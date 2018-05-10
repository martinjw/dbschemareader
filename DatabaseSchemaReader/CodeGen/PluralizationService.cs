using PluralizationService;
using PluralizationService.English;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DatabaseSchemaReader.CodeGen
{
    public class PluralizationServiceInstance
    {
        private readonly IPluralizationApi Api;
        private readonly CultureInfo CultureInfo;

        public PluralizationServiceInstance()
        {
            var builder = new PluralizationApiBuilder();
            builder.AddEnglishProvider();

            Api = builder.Build();
            CultureInfo = new CultureInfo("en-US");
        }


        public string Pluralize(string name)
        {
            return Api.Pluralize(name, CultureInfo) ?? name;
        }

        public string Singularize(string name)
        {
            return Api.Singularize(name, CultureInfo) ?? name;
        }
    }
}
