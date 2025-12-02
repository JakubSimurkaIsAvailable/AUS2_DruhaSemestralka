using Aus2_Heap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class TestLinearHash
    {
        int blockSizeHash = 1;
        int blockSizeOverflow;
        string pathHash;
        string pathOverflow;
        string metaPathHash;
        string metaPathOverflow;
        LinearHash<Patient> linearHash;
        int uidCounter = 0;

        public TestLinearHash()
        {
            blockSizeHash = 150;
            blockSizeOverflow = 80;
            pathHash = "hashFile.bin";
            pathOverflow = "overflowFile.bin";
            metaPathHash = "metaHashFile.bin";
            metaPathOverflow = "metaOverflowFile.bin";
            linearHash = new LinearHash<Patient>(blockSizeHash, blockSizeOverflow, pathHash, pathOverflow, metaPathHash, metaPathOverflow);
        }

        public void Run()
        {
            Console.WriteLine(linearHash.ToString());
            Console.WriteLine("Press N to write number of random operations, press R to test one by one");
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            var rand = new Random(1);

            var liveRecords = new List<Patient>();
            var uids = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                int blockCount = linearHash.GetBlockCount();
                for (int i = 0; i < blockCount; i++)
                {
                    var records = linearHash.GetAllRecordsInBlock(i);
                    foreach (var r in records)
                    {
                        if (!string.IsNullOrEmpty(r.UniqueID) && !uids.Contains(r.UniqueID))
                        {
                            liveRecords.Add(r);
                            uids.Add(r.UniqueID);
                        }
                    }
                }
                uidCounter = liveRecords.Count;
            }
            catch { }

            if (char.ToUpperInvariant(choice) == 'N')
            {
                Console.WriteLine("Enter number of random operations to perform:");
                if (!int.TryParse(Console.ReadLine(), out int target) || target <= 0)
                {
                    Console.WriteLine("Invalid number. Aborting.");
                    return;
                }

                for (int op = 0; op < target; op++)
                {
                    int chance = rand.Next(0, 100);
                    
                        if (chance < 60)
                        {
                            // Insert
                            var patient = GenerateRandomPatient(rand, uids);
                        Console.WriteLine($"Inserting {patient.UniqueID}");
                            linearHash.Insert(patient);
                            // verify retrievable
                            var got = linearHash.Get(patient);
                            if (!patient.Equals(got))
                                throw new Exception("ERROR: Inserted value does not match retrieved value!");
                            liveRecords.Add(patient);
                            uids.Add(patient.UniqueID);
                            if(linearHash.CalculateMaxBlockIndex()!=linearHash.GetBlockCount())
                            {
                                throw new Exception("ERROR: Max block index does not match block count!");
                            }
                        Console.WriteLine($"[{op + 1}/{target}] Inserted {patient.UniqueID}");
                        }
                        else if (chance < 61)
                        {
                            // Delete
                            /*
                            if (liveRecords.Count == 0)
                            {
                                Console.WriteLine($"[{op + 1}/{target}] Delete skipped - no records");
                                continue;
                            }
                            int idx = rand.Next(liveRecords.Count);
                            var toDelete = liveRecords[idx];
                            Console.WriteLine($"Deleting {toDelete.UniqueID}");
                        linearHash.Delete(toDelete);
                            // ensure deleted cannot be retrieved
                            bool exists = true;
                            try
                            {
                                var check = linearHash.Get(toDelete);
                                exists = true;
                            }
                            catch
                            {
                                exists = false;
                            }
                            if (exists)
                                throw new Exception("ERROR: Deleted value still retrievable!");
                            // remove from bookkeeping
                            liveRecords.RemoveAt(idx);
                            uids.Remove(toDelete.UniqueID);
                            Console.WriteLine($"[{op + 1}/{target}] Deleted {toDelete.UniqueID}");
                            */
                        }
                        else
                        {
                            // Get
                            if (liveRecords.Count == 0)
                            {
                                Console.WriteLine($"[{op + 1}/{target}] Get skipped - no records");
                                continue;
                            }
                            var chosen = liveRecords[rand.Next(liveRecords.Count)];
                            Console.WriteLine($"Getting {chosen.UniqueID}");
                        var got = linearHash.Get(chosen);
                            if (!chosen.Equals(got))
                                throw new Exception("ERROR: Retrieved value does not match expected value!");
                            Console.WriteLine($"[{op + 1}/{target}] Retrieved {got.UniqueID}");
                        }

                        // print some meta info
                        Console.WriteLine($"Block count: {linearHash.GetBlockCount()}");
                        Console.Write("Free blocks: ");
                        foreach (var f in linearHash.GetFreeBlocks()) Console.Write($"{f} ");
                        Console.WriteLine();
                        Console.Write("Not full blocks: ");
                        foreach (var n in linearHash.GetNotFullBlocks()) Console.Write($"{n} ");
                        Console.WriteLine();
                    
                }

                // pretend ToString exists and writes heapfile representation
                Console.WriteLine("Final LinearHash state (ToString output):");
                Console.WriteLine(linearHash.ToString());
                linearHash.Close();
            }
            else if (char.ToUpperInvariant(choice) == 'R')
            {
                Console.WriteLine("Step mode. Press Space to perform next operation, Esc to stop.");
                bool stop = false;
                while (!stop)
                {
                    int chance = rand.Next(0, 100);
                    try
                    {
                           if (chance < 60)
                        {
                            var patient = GenerateRandomPatient(rand, uids);
                            Console.WriteLine($"Inserting {patient.UniqueID}");
                            int blockId = linearHash.Insert(patient);
                            var got = linearHash.Get(patient);
                            if (!patient.Equals(got))
                                throw new Exception("ERROR: Inserted value does not match retrieved value!");
                            liveRecords.Add(patient);
                            uids.Add(patient.UniqueID);
                            if (linearHash.CalculateMaxBlockIndex() != linearHash.GetBlockCount())
                            {
                                Console.WriteLine(linearHash.CalculateMaxBlockIndex());
                                Console.WriteLine(linearHash.GetBlockCount());
                                throw new Exception("ERROR: Max block index does not match block count!");
                            }
                            Console.WriteLine($"Inserted {patient.UniqueID} to block {blockId}");
                        }
                        else if (chance < 61)
                        {
                            /*
                            if (liveRecords.Count == 0)
                            {
                                Console.WriteLine("Delete skipped - no records");
                            }
                            else
                            {
                                int idx = rand.Next(liveRecords.Count);
                                var toDelete = liveRecords[idx];
                                Console.WriteLine($"Deleting {toDelete.UniqueID}");
                                linearHash.Delete(toDelete);
                                bool exists = true;
                                try
                                {
                                    var check = linearHash.Get(toDelete);
                                    exists = true;
                                }
                                catch
                                {
                                    exists = false;
                                }
                                if (exists)
                                    throw new Exception("ERROR: Deleted value still retrievable!");
                                liveRecords.RemoveAt(idx);
                                uids.Remove(toDelete.UniqueID);
                                Console.WriteLine($"Deleted {toDelete.UniqueID}");
                            }
                            */
                        }
                        else
                        {
                            if (liveRecords.Count == 0)
                            {
                                Console.WriteLine("Get skipped - no records");
                            }
                            else
                            {
                                var chosen = liveRecords[rand.Next(liveRecords.Count)];
                                Console.WriteLine($"Getting {chosen.UniqueID}");
                                var got = linearHash.Get(chosen);
                                if (!chosen.Equals(got))
                                    throw new Exception("ERROR: Retrieved value does not match expected value!");
                                Console.WriteLine($"Retrieved {got.UniqueID}");
                            }
                        }

                        Console.WriteLine($"Block count: {linearHash.GetBlockCount()}");
                        Console.Write("Free blocks: ");
                        foreach (var f in linearHash.GetFreeBlocks()) Console.Write($"{f} ");
                        Console.WriteLine();
                        Console.Write("Not full blocks: ");
                        foreach (var n in linearHash.GetNotFullBlocks()) Console.Write($"{n} ");
                        Console.WriteLine();
                        Console.WriteLine(linearHash.ToString());
                        
                        while (true)
                        {
                             var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Spacebar) break;
                            if (key.Key == ConsoleKey.Escape)
                            {
                                stop = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Operation failed: {ex.Message}");
                        Console.WriteLine("Press Space to continue, Esc to stop.");
                        
                        while (true)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Spacebar)
                            {
                                //Console.WriteLine(linearHash.ToString());
                                break;
                            }
                            if (key.Key == ConsoleKey.Escape)
                            {
                                stop = true;
                                break;
                            }
                        }
                        
                    }
                }

                Console.WriteLine("Final LinearHash state (ToString output):");
                Console.WriteLine(linearHash.ToString());
                linearHash.Close();
            }
            else
            {
                Console.WriteLine("Unknown choice. Exiting.");
            }
        }

        private Patient GenerateRandomPatient(Random rand, HashSet<string> existingUids)
        {
            var mena = new[] { "Jan", "Peter", "Marek", "Tomas", "Lukas", "Martin", "Filip", "Michal", "Andrej", "Roman", "Maria", "Anna", "Eva", "Petra", "Lucia" };
            var priezviska = new[] { "Novak", "Kovac", "Horvath", "Varga", "Toth", "Balaz", "Kucera", "Molnar", "Bartos", "Svec", "Svoboda", "Micka", "Urban" };
            string name = mena[rand.Next(mena.Length)];
            string surname = priezviska[rand.Next(priezviska.Length)];
            DateTime birth = DateTime.Now.AddYears(-rand.Next(1, 90)).AddDays(-rand.Next(0, 365));
            
            string uid = uidCounter.ToString();
            uidCounter++;
            /*
            do
            {
                uid = Guid.NewGuid().ToString("N").ToUpperInvariant().Substring(0, 10);
            } while (existingUids.Contains(uid));
            */
            return new Patient(name, surname, birth, uid);
        }

        
    }
}
