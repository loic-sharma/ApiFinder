using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace ApiFinder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide the path to a portable executable");
                return;
            }

            using var stream = new FileStream(args[0], FileMode.Open);
            using var peReader = new PEReader(stream);

            var reader = peReader.GetMetadataReader();

            foreach (var memberReferenceHandle in reader.MemberReferences)
            {
                var memberReference = reader.GetMemberReference(memberReferenceHandle);
                if (memberReference.Parent.Kind == HandleKind.TypeSpecification)
                {
                    continue;
                }

                if (memberReference.Parent.Kind != HandleKind.TypeReference)
                {
                    throw new InvalidOperationException(
                        $"Unknown member parent kind {memberReference.Parent.Kind}");
                }

                var typeReference = reader.GetTypeReference((TypeReferenceHandle)memberReference.Parent);

                var typeName = reader.GetString(typeReference.Name);
                var typeNamespace = reader.GetString(typeReference.Namespace);
                var memberName = reader.GetString(memberReference.Name);

                Console.WriteLine($"Found method {memberName} on {typeName} in namespace {typeNamespace}");
            }
        }

    }
}
