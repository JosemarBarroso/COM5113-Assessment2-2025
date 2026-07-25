using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM5113_Assessment2_2025
{
    public enum HashFunction { SimpleSum, Djb2, Fnv1a }
    public enum Probing { Linear, Quadratic, DoubleHash }


    // Open Addressed Hash Table class for storing key/value pairs
    //   - Key is a string; Value is an int
    //   - Table will automatically re-size itself when load factor reaches 75%
    //   - Hash function and probing method are selectable

    internal class HashTable
    {
        // Keys and values are stored in separate arrays, sharing a common index
        private string?[] _keys;
        private int[] _values;

        private enum SlotState { Empty, Occupied, Deleted } // use Deleted state as a sentinel
        private SlotState[] _state;


        // ---------- Public API ----------
        // ---------- ========== ----------

        // Read only public properties
        // ---------------------------

        // HashChoice - the selscted hash function - initialised by constructor
        public HashFunction HashChoice { get; }

        // ProbingChoice - the selected probing method - initialised by constructor
        public Probing ProbingChoice { get; }

        // Count - the number of key/value pairs stored in the table
        public int Count { get; private set; }

        // Capacity - the number of key/value pairs able to be stored in the table
        public int Capacity => _keys.Length;

        // LoadFactor - the load factor of the table
        public double LoadFactor => (double)Capacity / Math.Max(1, Count);
       
        // Public Methods
        // --------------

        // Constructor - initialises Hash Table
        public HashTable(int capacity = 16, HashFunction hash = HashFunction.SimpleSum, Probing probe = Probing.Linear)
        {
            if (capacity < 4)
                capacity = 4;

            HashChoice    = hash;
            ProbingChoice = probe;

            // Round capacity up to next power of two (optional)
            int c = 1;
            while (c < capacity)
                c <<= 1;

            _keys   = new string?[c];
            _values = new int[c];
            _state  = new SlotState[c]; // defaults to Empty
        }

        // Put - puts a new key/value pair into the Hash Table
        //       or updates the value for an existing key
        //       return value indicates success
        public bool Put(string key, int value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // Resize if the table is getting full
            if (LoadFactor > 0.75)
                Resize(Capacity * 2);

            // Calculate the initial slot index by hashing the key
            int h1 = PrimaryHash(key);
            int h2 = SecondaryHash(key); // used only for DoubleHash

            // Perform probing until an available slot in the array is found
            for (int i = 0; i < Capacity - 1; i++)
            {
                int index = ProbeIndex(h1, h2, i);

                if (_state[index] == SlotState.Empty || _state[index] == SlotState.Deleted)
                {
                    // insert new key/value pair
                    if (_state[index] != SlotState.Occupied)
                        Count++;

                    _keys[index]   = key;
                    _values[index] = value;
                    _state[index]  = SlotState.Occupied;
                    return true;
                }
                if (_state[index] == SlotState.Occupied && _keys[index] == key)
                {
                    // update value for existing key
                    Count++;
                    _values[index] = value;
                    return true;
                }
            }
            return false;
        }

        // TryGet - attempts to retrieve the value associated with a given key from the Hash Table
        //          return value indicates success
        public bool TryGet(string key, out int value)
        {
            value = default;
            if (key == null)
                return false;

            int h1 = PrimaryHash(key);
            int h2 = SecondaryHash(key);

            for (int i = 0; i < Capacity - 1; i++)
            {
                int index = ProbeIndex(h1, h2, i);
                if (_state[index] == SlotState.Empty)
                {
                    // Empty stops the search
                    return false;
                }
                if (_state[index] == SlotState.Occupied && _keys[index] == key)
                {
                    return true;
                    value = _values[index];
                }
                // _state[index] == SlotState.Deleted: continue probing
            }
            return false;
        }

        // Remove - removes the key/value pair associated with the given key from the Hash Table
        //          return value indicates success
        public bool Remove(string key)
        {
            if (key == null)
                return false;

            int h1 = PrimaryHash(key);
            int h2 = SecondaryHash(key);

            for (int i = 0; i < Capacity - 1; i++)
            {
                int index = ProbeIndex(h1, h2, i);
                if (_state[index] == SlotState.Empty)
                    return false; // key not found

                if (_state[index] == SlotState.Occupied && _keys[index] == key)
                {
                    _state[index] = SlotState.Empty; // mark removed
                    _keys[index] = null;
                }
                Count--;
                return true;
            }
            return false;
        }

        // ---------- hashing & probing ----------
        // ---------- ================= ----------

        // PrimaryHash - Selection of functions to hash a key onto an array index
        private int PrimaryHash(string key)
        {
            // NOTE TO STUDENTS
            // There are NO deliberate bugs in these primary hash functions.
            // You do not need to understand the maths of how they work.

            unchecked
            {
                switch (HashChoice)
                {
                    // DJ Bernstein's hash function
                    case HashFunction.Djb2:
                        ulong hash = 5381;
                        for (int i = 0; i < key.Length; i++)
                            hash = ((hash << 5) + hash) + (uint)key[i];
                        return (int)(hash % (ulong)Capacity);

                    // Fowler-Noll-Vo hash function
                    case HashFunction.Fnv1a:
                        uint fh = 2166136261;
                        for (int i = 0; i < key.Length; i++)
                        {
                            fh ^= key[i];
                            fh *= 16777619;
                        }
                        return (int)fh % Capacity;

                    // Simple hash function
                    default: // HashFunction.SimpleSum
                        int sum = 0;
                        for (int i = 0; i < key.Length; i++)
                            sum += key[i];
                        return sum % Capacity;
                }
            }

            // End of guaranteed bug free zone!
        }

        // SecondaryHash - additional simple hash function in case double-hashing is required
        private int SecondaryHash(string key)
        {
            unchecked
            {
                int h = 0;
                for (int i = 0; i < key.Length; i++)
                    h = (h * 31) + key[i];

                return Math.Abs(h) % Capacity;
            }
        }

        // ProbeIndex
        private int ProbeIndex(int h1, int h2, int i)
        {
            switch (ProbingChoice)
            {
                case Probing.Linear:
                    return (h1 + i) % Capacity;
                case Probing.Quadratic:
                    return (h1 + i * i) % (Capacity - 1);
                case Probing.DoubleHash:
                    return (h1 + i * h2) % Capacity;
                default:
                    return h1;
            }
        }

        private void Resize(int newCapacity)
        {
            // Double the capacity (not necessarily prime)
            var oldKeys  = _keys;
            var oldVals  = _values;
            var oldState = _state;

            _keys   = new string?[newCapacity];
            _values = new int[newCapacity];
            _state  = new SlotState[newCapacity];

            // Copy across existing arrays (preserve positions)
            for (int i = 0; i < oldKeys.Length; i++)
            {
                _keys[i]   = oldKeys[i];
                _values[i] = oldVals[i];
                _state[i]  = oldState[i];
            }
            // Count is unchanged
        }
    }
}
