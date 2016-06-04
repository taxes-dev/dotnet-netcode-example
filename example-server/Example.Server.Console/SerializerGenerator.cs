using System;
using System.Reflection;

using ProtoBuf.Meta;

namespace Example.Server.Console
{
    using Console = System.Console;

    public static class SerializerGenerator
    {
        /// <summary>
        /// The list of assemblies to gather types from.
        /// </summary>
        private static string[] ASSEMBLY_NAMES = { "Example.GameStructures", "Example.Messages" };

        /// <summary>
        /// Generates the serializers needed for protobuf serialization of message types.
        /// </summary>
        public static void Generate()
        {
            var model = TypeModel.Create();

            foreach (string assemblyName in ASSEMBLY_NAMES)
            {
                Console.WriteLine("Reading types from {0} ...", assemblyName);

                // 1. Load the assembly
                Assembly assembly = Assembly.Load(assemblyName);

                // 2. Iterate through public types
                foreach (Type type in assembly.GetExportedTypes())
                {
                    // 3. Look for protobuf markers on the type
                    object[] attr = type.GetCustomAttributes(typeof(ProtoBuf.ProtoContractAttribute), false);

                    // 4. If we found a protobuf contract, add it to the model
                    if (attr.Length > 0)
                    {
                        Console.WriteLine("Adding {0}", type.FullName);
                        model.Add(type, true);
                    }
                }
                Console.WriteLine();
            }

            // 5. Compile all the found types' serializers into a separate assembly
            model.Compile("Example.Serializer", "Example.Serializer.dll");
        }
    }
}
