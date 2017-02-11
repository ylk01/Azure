using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace TableStorage
{
    public class CustomerEntity : TableEntity
    {
        public CustomerEntity(string Country, string firstName)
        {
            this.PartitionKey = Country;
            this.RowKey = firstName;
        }

        public CustomerEntity() { }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 01 - Connect to your azure storage account

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Console.WriteLine("Connected to Account");

            // 02 - Create a table called "customers"

            CloudTable table = tableClient.GetTableReference("customers");
            table.CreateIfNotExists();
            Console.WriteLine("Created Table");

            // 03 - Insert single entity (instance of the CustomerEntity class) into the table

            CustomerEntity customer1 = new CustomerEntity("Russia", "Walter");
            customer1.Email = "Walter@contoso.com";
            customer1.PhoneNumber = "425-555-0101";

            table.Execute(TableOperation.Insert(customer1));

            // 04 - Insert two additional CustomerEntity objects using batching (use PartitionKey "Netherlands")

            TableBatchOperation batchOperation = new TableBatchOperation();

            CustomerEntity customer2 = new CustomerEntity("Netherlands", "Jeff");
            customer1.Email = "Jeff@contoso.com";
            customer1.PhoneNumber = "425-555-0104";

            CustomerEntity customer3 = new CustomerEntity("Netherlands", "Ben");
            customer2.Email = "Ben@contoso.com";
            customer2.PhoneNumber = "425-555-0102";

            batchOperation.Insert(customer1);
            batchOperation.Insert(customer2);

            table.ExecuteBatch(batchOperation);

            // 05 - Retrieve one of the entities using TableOperation.Retrieve and print its PartitionKey using Console.WriteLine()

            TableResult retrievedResult = table.Execute(TableOperation.Retrieve<CustomerEntity>("Russia", "Walter"));

            if (retrievedResult.Result != null) Console.WriteLine(((CustomerEntity)retrievedResult.Result).PhoneNumber);
            else Console.WriteLine("The phone number could not be retrieved.");

            // 06 - Retrieve all entities with PartitionKey "Netherlands" using TableQuery and print their RowKey using Console.WriteLine()

            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"));

            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Console.WriteLine("{0}, {1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
            }

            // 07 - Delete one of the entities from the table 

            CustomerEntity deleteEntity = (CustomerEntity)retrievedResult.Result;

            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                table.Execute(deleteOperation);
                Console.WriteLine("Entity deleted.");
            }
            else
                Console.WriteLine("Could not retrieve the entity.");

            // 08 - Insert a new entity into "customers" using DynamicTableEntity instead of CustomerEntity

            DynamicTableEntity entryEntity = new DynamicTableEntity("Netherlands", "Ivan");

            entryEntity.Properties["Email"] = new EntityProperty("test@mail.ru");
            entryEntity.Properties.Add("Phone", new EntityProperty("10009-90-90"));

            Console.WriteLine("Dynamic entity added.");

            Console.ReadLine();


        }
    }
}
