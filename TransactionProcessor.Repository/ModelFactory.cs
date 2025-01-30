using TransactionProcessor.Models.Estate;

namespace TransactionProcessor.Repository
{
    using EstateModel = Models.Estate.Estate;
    using EstateEntity = Database.Entities.Estate;
    using EstateSecurityUserEntity = Database.Entities.EstateSecurityUser;
    using EstateOperatorModel = Models.Estate.Operator;
    using SecurityUserModel = SecurityUser;
    using OperatorEntity = Database.Entities.Operator;

    public static class ModelFactory
    {
        public static EstateModel ConvertFrom(EstateEntity estate,
                                       List<EstateSecurityUserEntity> estateSecurityUsers,
                                       List<OperatorEntity> operators)
        {
            EstateModel estateModel = new EstateModel();
            estateModel.EstateId = estate.EstateId;
            estateModel.EstateReportingId = estate.EstateReportingId;
            estateModel.Name = estate.Name;
            estateModel.Reference = estate.Reference;

            if (operators != null && operators.Any())
            {
                estateModel.Operators = new List<EstateOperatorModel>();
                foreach (OperatorEntity @operator in operators)
                {
                    estateModel.Operators.Add(new EstateOperatorModel
                    {
                        OperatorId = @operator.OperatorId,
                        Name = @operator.Name
                    });
                }
            }

            if (estateSecurityUsers != null && estateSecurityUsers.Any())
            {
                estateModel.SecurityUsers = new List<SecurityUserModel>();
                estateSecurityUsers.ForEach(esu => estateModel.SecurityUsers.Add(new SecurityUserModel
                {
                    SecurityUserId = esu.SecurityUserId,
                    EmailAddress = esu.EmailAddress
                }));
            }

            return estateModel;
        }
    }
}
