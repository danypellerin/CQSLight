using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using System;
using System.Linq;

namespace PGMS.DataProvider.EFCore.Services
{
    public class DataService<TDbContext> : IDataService where TDbContext : BaseDbContext
    {
        private readonly string ParameterName;
        private readonly BaseEntityRepository<TDbContext> entityRepository;
        private readonly string schema;
        private static readonly object SyncRoot = new object();
        private readonly int maxLo = 1000;
        private long currentHi = -1;
        private int currentLo;

        public DataService(BaseEntityRepository<TDbContext> entityRepository, string appName = "Default", string schema = null)
        {
            this.entityRepository = entityRepository;
            this.schema = schema;
            ParameterName = appName;
        }

        public long GenerateId()
        {
            long result;
            lock (SyncRoot)
            {
                if (currentHi == -1)
                {
                    MoveNextHi();
                }
                if (currentLo == maxLo)
                {
                    currentLo = 0;
                    MoveNextHi();
                }
                result = (currentHi * maxLo) + currentLo;
                currentLo++;
            }
            return result;
        }

        public void InitHiLoTable()
        {
            try
            {
                GenerateId();
            }
            catch (Exception)
            {
                try
                {
                    var tablename = string.IsNullOrEmpty(schema) ? "SequenceHiLo" : $"[{schema}].SequenceHiLo";
                    entityRepository.ExecuteSqlCommand(entityRepository.GetUnitOfWork(), $"INSERT INTO {tablename}([id_parametres], [intval]) values ('{ParameterName}', 9)");
                }
                catch (Exception)
                {
                    //Database may not be initialized - Do nothing, will be initialized next time
                }
            }
        }

        private void MoveNextHi()
        {
            var tableName = string.IsNullOrEmpty(schema) ? "SequenceHiLo" : $"[{schema}].SequenceHiLo";
            var updateQuery = $"UPDATE {tableName} SET intval=intval+1 WHERE id_parametres='{ParameterName}';";
            var getValueQuery = $"SELECT id, intval, id_parametres FROM {tableName} where id_parametres='{ParameterName}';";

            using var unitOfWork = entityRepository.GetUnitOfWork();
            using var transaction = unitOfWork.GetTransaction();
            entityRepository.ExecuteSqlCommand(unitOfWork, updateQuery);
            var nextId = ((TDbContext)unitOfWork.GetDbContext()).SequenceHiLo.FromSqlRaw(getValueQuery).AsNoTracking().ToList();
            transaction.Commit();
            currentHi = nextId.First().IntVal;
        }
    }
}