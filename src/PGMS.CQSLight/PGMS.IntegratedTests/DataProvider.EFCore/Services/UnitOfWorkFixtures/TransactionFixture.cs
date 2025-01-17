﻿using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;
using System;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.UnitOfWorkFixtures
{
    [TestFixture]
    public class TransactionFixture
    {
        private IEntityRepository entityRepository;
        private readonly string connectionString = "Server=localhost;Database=SampleProject;Trusted_Connection=True;ConnectRetryCount=0";

        [SetUp]
        public void SetUp()
        {
            entityRepository = new BaseEntityRepository<BaseDbContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
        }

        [Test]
        public void CheckTransactionNewInsertedItem()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTest-{id}";

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                entityRepository.InsertOperation(unitOfWork, new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(value, Is.Not.Null);
                transaction.Rollback();
            }

            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckTransactionNewInsertedItemPersisted()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTest-Persisted-{id}";

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                entityRepository.InsertOperation(unitOfWork, new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(value, Is.Not.Null);
                transaction.Commit();
            }

            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CheckTransactionUpdatedItem()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTest-update-{id}";

            entityRepository.Insert(new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                value.IntVal = 100;
                entityRepository.UpdateOperation(unitOfWork, value);

                var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(updated.IntVal, Is.EqualTo(100));

                transaction.Rollback();
            }
            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result.IntVal, Is.EqualTo(1));
        }

        [Test]
        public void CheckTransactionUpdatedItemPersisted()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTestPersisted-update-{id}";

            entityRepository.Insert(new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                value.IntVal = 100;
                entityRepository.UpdateOperation(unitOfWork, value);

                var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(updated.IntVal, Is.EqualTo(100));

                transaction.Commit();
            }

            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result.IntVal, Is.EqualTo(100));
        }

        [Test]
        public void CheckTransactionDeletedItem()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTest-delete-{id}";

            entityRepository.Insert(new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                value.IntVal = 100;
                entityRepository.DeleteOperation(unitOfWork, value);

                var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(updated, Is.Null);

                transaction.Rollback();
            }
            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result.IntVal, Is.EqualTo(1));
        }

        [Test]
        public void CheckTransactionDeletedItemPersisted()
        {
            var id = DateTime.Now.ToEpoch();
            var idParameters = $"IntegratedTestPersisted-delete-{id}";

            entityRepository.Insert(new DbSequenceHiLo { IdParameters = idParameters, IntVal = 1 });

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using var transaction = unitOfWork.GetTransaction();
                var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                value.IntVal = 100;
                entityRepository.DeleteOperation(unitOfWork, value);

                var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.IdParameters == idParameters);
                Assert.That(updated, Is.Null);

                transaction.Commit();
            }

            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.IdParameters == idParameters);
            Assert.That(result, Is.Null);
        }
    }

    public class IntegratedTestContextFactory : ContextFactory<BaseDbContext>
    {
        private bool reUseContext = false;
        private BaseDbContext context = null;

        public override BaseDbContext CreateContext(DbContextOptions<BaseDbContext> options)
        {
            if (reUseContext == false)
            {
                return new BaseDbContext(options);
            }
            if (context != null)
            {
                return context;
            }
            context = new BaseDbContext(options);
            return context;
        }

        public void InitContextUsage(bool reUseContext)
        {
            this.reUseContext = reUseContext;
        }
    }
}