namespace COM5113_Assessment2_2025
{
    internal class Program
    {
        // Here is a little class with some static methods to help validate results
        // and produce some nice readable console output.
        static class Validate
        {
            // Check whether a boolean is true
            public static void Check(string name, bool condition)
                => Console.WriteLine($"{(condition ? "PASS" : "FAIL")}: {name}");

            // Compare two values
            public static void Equal<T>(string name, T expected, T actual)
                => Check($"{name} (expected {expected}, got {actual})", Equals(expected, actual));
        }

        static void Main()
        {
            Console.WriteLine("== Test Harness for HashTable ==");

            TestScenario1();
            TestScenario2();
            TestScenario3();

            Console.WriteLine("\nDone.");
        }


        // Here is a test scenario, which will expose the first bug
        static void TestScenario1()
        {
            var t = new HashTable(8, HashFunction.Djb2, Probing.Linear);

            // Test 1) Insert then TryGet the same key
            bool ok1 = t.Put("Nick", 42);
            bool ok2 = t.TryGet("Nick", out var v1);

            Validate.Check("Put returns true", ok1);
            Validate.Check("TryGet returns true for existing key", ok2);
            Validate.Equal("TryGet returns the stored value", 42, v1);

            // Test 2) Update value for existing key then TryGet the same key
            t.Put("Nick", 99);
            bool ok3 = t.TryGet("Nick", out var v2);
            Validate.Check("TryGet returns true after update", ok3);
            Validate.Equal("TryGet returns the updated value", 99, v2);

            // 3) TryGet value for missing key
            bool ok4 = t.TryGet("missing", out var v3);
            Validate.Check("TryGet returns false for missing key", !ok4);
        }

        static void TestScenario2()
        {
            Console.WriteLine("\n-- Test Scenario 2: Load Factor and Capacity --");

            var t = new HashTable(8, HashFunction.Djb2, Probing.Linear);

            Validate.Equal("Initial count is zero", 0, t.Count);
            Validate.Equal("Initial capacity is 8", 8, t.Capacity);
            Validate.Equal("Initial load factor is 0", 0.0, t.LoadFactor);

            t.Put("A", 1);

            Validate.Equal("Count is 1 after insertion", 1, t.Count);
            Validate.Equal(
                "Capacity remains 8 after one insertion",
                8,
                t.Capacity
            );
            Validate.Equal(
                "Load factor is 0.125 after one insertion",
                0.125,
                t.LoadFactor
            );
        }

        static void TestScenario3()
        {
            Console.WriteLine("\n-- Test Scenario 3: Count After Updating Existing Key --");

            var t = new HashTable(8, HashFunction.Djb2, Probing.Linear);

            t.Put("Nick", 42);

            Validate.Equal(
                "Count is 1 after first insertion",
                1,
                t.Count
            );

            t.Put("Nick", 99);

            Validate.Equal(
                "Count remains 1 after updating existing key",
                1,
                t.Count
            );

            bool found = t.TryGet("Nick", out int value);

            Validate.Check(
                "Updated key remains searchable",
                found
            );

            Validate.Equal(
                "Updated key stores the new value",
                99,
                value
            );
        }

    }
}
