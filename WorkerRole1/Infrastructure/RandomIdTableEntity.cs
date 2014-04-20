using System;

using Microsoft.WindowsAzure.Storage.Table;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class RandomIdTableEntity : TableEntity
    {
        public RandomIdTableEntity(int clusteringIndex)
            : base((DateTime.Now.Millisecond % clusteringIndex).ToString(), Guid.NewGuid().ToString())
        {
        }

        public RandomIdTableEntity()
            : this(10)
        {
        }
    }
}