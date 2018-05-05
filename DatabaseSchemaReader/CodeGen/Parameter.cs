// TODO: pluralize the collection properties and With methods that use them
// TODO: figure out how to properly overload the getlist methods for cao by parent and caoid
// TODO: figure out how to handle nullable parameters for the wither's
// TODO: figure out how to auto-gen the body of each With* method so as to use thisDOT properties
// TODO: figure out how to write GetList methods even when there is no foreignkey to base off of (e.g., we need customerdevicegroup.getlist(cid, gid), cvg.getlist(cid, gid), landmarkgroup
// TODO: what if we did this: get a list of all primary keys for the class, write a GetSingle with parameters for each primary key, and then for each combination of primary keys write a GetList with parameters for each combination, with uniquenames going by "GetByCustomerGroupId" or something, thus handling the overload problem
namespace DatabaseSchemaReader.CodeGen
{
    public struct Parameter
    {
        public string Name;
        public string DataType;
        public string ColumnNameToQueryBy;
        public string Summary;
    }
}
