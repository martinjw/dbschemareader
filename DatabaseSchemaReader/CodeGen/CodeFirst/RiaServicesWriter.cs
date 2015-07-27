namespace DatabaseSchemaReader.CodeGen.CodeFirst
{
    static class RiaServicesWriter
    {
        public static void WritePrivateConstructor(string className, ClassBuilder cb)
        {
                using (cb.BeginNest("private " + className + "Metadata()"))
                {
                    cb.AppendLine("// Metadata classes are not meant to be instantiated.");
                }
        }
    }
}
