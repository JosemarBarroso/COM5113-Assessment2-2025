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
            TestScenario4();
            TestScenario5();
            TestScenario6();
            TestScenario7();
            TestScenario8();
            TestScenario9();

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

        static void TestScenario4()
        {
            Console.WriteLine("\n-- Test Scenario 4: Remove Missing Colliding Key --");

            var t = new HashTable(8, HashFunction.SimpleSum, Probing.Linear);

            t.Put("A", 10);

            // "I" hashes to the same initial slot as "A" but was never inserted.
            bool removed = t.Remove("I");

            Validate.Check(
                "Remove returns false for missing collided key",
                !removed
            );

            Validate.Equal(
                "Count remains 1 after failed removal",
                1,
                t.Count
            );

            bool found = t.TryGet("A", out int value);

            Validate.Check(
                "Existing key remains searchable",
                found
            );

            Validate.Equal(
                "Existing value remains unchanged",
                10,
                value
            );
        }

        static void TestScenario5()
        {
            Console.WriteLine(
                "\n-- Test Scenario 5: Remove One Key From Collision Chain --"
            );

            var t = new HashTable(
                8,
                HashFunction.SimpleSum,
                Probing.Linear
            );

            // Both keys hash to index 1.
            t.Put("A", 10);
            t.Put("I", 20);

            bool removed = t.Remove("A");
            bool foundI = t.TryGet("I", out int valueI);

            Validate.Check(
                "Remove returns true for existing collided key",
                removed
            );

            Validate.Check(
                "Second collided key remains searchable",
                foundI
            );

            Validate.Equal(
                "Second collided key retains its value",
                20,
                valueI
            );

            Validate.Equal(
                "Count is 1 after removing one of two keys",
                1,
                t.Count
            );
        }

        static void TestScenario6()
        {
            Console.WriteLine(
                "\n-- Test Scenario 6: Resize and Rehash --"
            );

            var t = new HashTable(
                4,
                HashFunction.SimpleSum,
                Probing.Linear
            );

            t.Put("A", 1);
            t.Put("B", 2);
            t.Put("C", 3);
            t.Put("D", 4);

            // The fifth insertion triggers resizing from 4 to 8.
            bool insertedE = t.Put("E", 5);

            Validate.Check(
                "Fifth key is inserted successfully",
                insertedE
            );

            Validate.Equal(
                "Capacity increases from 4 to 8",
                8,
                t.Capacity
            );

            bool foundA = t.TryGet("A", out int valueA);
            bool foundB = t.TryGet("B", out int valueB);
            bool foundC = t.TryGet("C", out int valueC);
            bool foundD = t.TryGet("D", out int valueD);
            bool foundE = t.TryGet("E", out int valueE);

            Validate.Check("Key A remains searchable after resize", foundA);
            Validate.Equal("Key A retains value", 1, valueA);

            Validate.Check("Key B remains searchable after resize", foundB);
            Validate.Equal("Key B retains value", 2, valueB);

            Validate.Check("Key C remains searchable after resize", foundC); 
            Validate.Equal("Key C retains value", 3, valueC);

            Validate.Check("Key D remains searchable after resize", foundD);
            Validate.Equal("Key D retains value", 4, valueD);

            Validate.Check("Key E is searchable after resize", foundE);
            Validate.Equal("Key E retains value", 5, valueE);

            Validate.Equal(
                "Count remains five after resize",
                5,
                t.Count
            );
        }

        static void TestScenario7()
        {
            Console.WriteLine(
                "\n-- Test Scenario 7: Quadratic Probing --"
            );

            var t = new HashTable(
                8,
                HashFunction.SimpleSum,
                Probing.Quadratic
            );

            Validate.Check("Insert A", t.Put("A", 1));
            Validate.Check("Insert I", t.Put("I", 2));
            Validate.Check("Insert Q", t.Put("Q", 3));
            Validate.Check("Insert Y", t.Put("Y", 4));

            bool foundA = t.TryGet("A", out int valueA);
            bool foundI = t.TryGet("I", out int valueI);
            bool foundQ = t.TryGet("Q", out int valueQ);
            bool foundY = t.TryGet("Y", out int valueY);

            Validate.Check("Find A", foundA);
            Validate.Equal("Value A", 1, valueA);

            Validate.Check("Find I", foundI);
            Validate.Equal("Value I", 2, valueI);

            Validate.Check("Find Q", foundQ);
            Validate.Equal("Value Q", 3, valueQ);

            Validate.Check("Find Y", foundY);
            Validate.Equal("Value Y", 4, valueY);
        }

        static void TestScenario8()
        {
            Console.WriteLine(
                "\n-- Test Scenario 8: Double Hash Collision Handling --"
            );

            var t = new HashTable(
                8,
                HashFunction.SimpleSum,
                Probing.DoubleHash
            );

            // Empty string produces secondary hash 0.
            bool firstInserted = t.Put("", 10);

            // This key has the same primary hash index as the empty string
            // with SimpleSum and capacity 8.
            bool secondInserted = t.Put("H", 20);

            Validate.Check(
                "First double-hash key is inserted",
                firstInserted
            );

            Validate.Check(
                "Second colliding key is inserted",
                secondInserted
            );

            bool foundFirst = t.TryGet("", out int firstValue);
            bool foundSecond = t.TryGet("H", out int secondValue);

            Validate.Check(
                "First key remains searchable",
                foundFirst
            );

            Validate.Equal(
                "First key retains its value",
                10,
                firstValue
            );

            Validate.Check(
                "Second colliding key is searchable",
                foundSecond
            );

            Validate.Equal(
                "Second colliding key retains its value",
                20,
                secondValue
            );

            Validate.Equal(
                "Count is 2 after two distinct insertions",
                2,
                t.Count
            );
        }

        static void TestScenario9()
        {
            Console.WriteLine("\n-- Test Scenario 9: Reinsert After Delete --");

            var t = new HashTable(8, HashFunction.SimpleSum, Probing.Linear);

            t.Put("A", 10);
            t.Remove("A");

            Validate.Equal(
                "Count is 0 after removal",
                0,
                t.Count
            );

            bool inserted = t.Put("A", 99);

            Validate.Check(
                "Key can be reinserted after deletion",
                inserted
            );

            bool found = t.TryGet("A", out int value);

            Validate.Check(
                "Reinserted key is searchable",
                found
            );

            Validate.Equal(
                "Reinserted key stores new value",
                99,
                value
            );

            Validate.Equal(
                "Count is 1 after reinsertion",
                1,
                t.Count
            );
        }

    }
}
