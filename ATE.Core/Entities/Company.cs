﻿using System.Collections.Generic;
using System.Reflection.Metadata;
using ATE.Core.Entities.ATE;
using ATE.Core.Entities.Billing;
using ATE.Core.Generators;

namespace ATE.Core.Entities
{
    public class Company : BaseEntity
    {
        private BillingSystem _billingSystem;
        private AutomaticTelephoneExchange _ate;
        public string Name { get; }
        public string CountryCode { get; }
        public string CompanyCode { get; }
        public List<Contract> Contracts { get; }
        public List<Tariff> Tariffs { get; }

        public Company(string name, string countryCode, string companyCode)
        {
            Name = name;
            CountryCode = countryCode;
            CompanyCode = companyCode;
        }
        
        public override string ToString()
        {
            return $"#{Id} Название: {Name}. Код страны: {CountryCode}. Код компании: {CompanyCode}";
        }

        public BillingSystem BillingSystem => _billingSystem ??= new BillingSystem();
        public AutomaticTelephoneExchange Ate => _ate ??= new AutomaticTelephoneExchange(this, 65536);

        public Contract CreateContract(Client client, Tariff tariff)
        {
            var phoneNumber = new PhoneNumberGenerator().Generate(company: this);
            var contract = new Contract(phoneNumber, tariff, client, this);
            Contracts.Add(contract);

            return contract;
        }
        
    }
}
