using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Estate
{
    public class CreateEstateRequest
    {
        public Guid EstateId { get; set; }

        public String EstateName { get; set; }
    }
}