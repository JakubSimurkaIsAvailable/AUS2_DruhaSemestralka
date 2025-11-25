using Aus2_Heap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class Tester
    {
        

        string testFilePath = "C:\\Users\\Jakub\\source\\repos\\AUS2_DruhaSemestralka\\Aus2_Heap\\TesterData\\numbers.bin";
        string metaFilePath = "C:\\Users\\Jakub\\source\\repos\\AUS2_DruhaSemestralka\\Aus2_Heap\\TesterData\\metadata.bin";
        int recordSize = 150;
        public Heap<Patient> heap;
        public List<long> ids = new List<long>();
        Random rand = new Random();
        List<List<Patient>> testDataList = new List<List<Patient>>();
        public Tester()
        {
            heap = new Heap<Patient>(recordSize, testFilePath, metaFilePath); 
        }
        public void Run()
        {
            var mena = new[] { "Jan", "Peter", "Marek", "Tomas", "Lukas", "Martin", "Filip", "Michal", "Andrej", "Roman", "Maria", "Anna", "Eva", "Petra", "Lucia" };
            var priezviska = new[] { "Novak", "Kovac", "Horvath", "Varga", "Toth", "Balaz", "Kucera", "Molnar", "Bartos", "Svec", "Svoboda", "Micka", "Urban" };
            int blockCount = heap.GetBlockCount();
            for (int i = 0; i < blockCount; i++)
            {
                testDataList.Add(new List<Patient>());
                var records = heap.GetAllRecordsInBlock(i);
                foreach (var record in records)
                {
                    testDataList[i].Add(record);
                    ids.Add(long.Parse(record.UniqueID));
                }
            }
            while (true)
            {
                Console.Clear();
                int chance = rand.Next(0, 100);
                if (chance < 60)
                {
                    Console.Write("Operation Insert: ");
                    int number = rand.Next();
                    var rok = rand.Next(1940, 2006);
                    var mesiac = rand.Next(1, 13);
                    var den = rand.Next(1, DateTime.DaysInMonth(rok, mesiac) + 1);
                    DateTime datumNarodenia = new DateTime(rok, mesiac, den);
                    string meno = mena[rand.Next(0, mena.Length)];
                    string priezvisko = priezviska[rand.Next(0, priezviska.Length)];
                    long uniqueID = rand.NextInt64(1000000000, 9999999999);
                    while (ids.Contains(uniqueID))
                    {
                        uniqueID = rand.NextInt64(1000000000, 9999999999);
                    }
                    ids.Add(uniqueID);
                    string uidString = uniqueID.ToString();
                    Patient data = new Patient(meno, priezvisko, datumNarodenia, uidString);
                    int addr = heap.Seek();
                    int validCountBeforeInsert;
                    try
                    {
                        validCountBeforeInsert = heap.GetValidCountInBlock(addr);
                    }
                    catch (Exception)
                    {
                        validCountBeforeInsert = 0;
                    }
                    heap.Insert(data);
                    if (testDataList.Count() - 1 < addr)
                    {
                        testDataList.Add(new List<Patient>());
                    }
                    testDataList[addr].Add(data);
                    var valueList = testDataList[addr].Last();
                    var valueHeap = heap.Get(addr, data);
                    if (!valueList.Equals(valueHeap) || !data.Equals(valueHeap) || !valueList.Equals(valueHeap))
                    {
                        throw new Exception("ERROR: Inserted value does not match retrieved value!");
                    }
                    if (testDataList.Count != heap.GetBlockCount())
                    {
                        throw new Exception("ERROR: Block count mismatch after insertion!");
                    }
                    if (heap.GetValidCountInBlock(addr) != validCountBeforeInsert + 1)
                    {
                        throw new Exception("ERROR: Valid count in block did not increase after insertion!");
                    }
                    Console.WriteLine($"Inserted number {number} at address {addr}");
                }
                else if (chance < 95)
                {
                    Console.Write("Operation Delete: ");
                    if (testDataList.Count == 0)
                    {
                        Console.WriteLine("No data to delete.");
                        continue;
                    }
                    int addr = rand.Next(0, testDataList.Count);
                    int validCountBeforeDelete = heap.GetValidCountInBlock(addr);
                    if (testDataList[addr].Count == 0)
                    {
                        Console.WriteLine($"No data to delete at block {addr}");
                        continue;
                    }
                    int dataIndex = rand.Next(0, testDataList[addr].Count);
                    Patient dataToDelete = testDataList[addr][dataIndex];
                    heap.Delete(addr, dataToDelete);
                    testDataList[addr].RemoveAt(dataIndex);
                    if (testDataList.Count > addr && testDataList[addr].Contains(dataToDelete))
                    {
                        throw new Exception("ERROR: Deleted value still present in test data list!");
                    }
                    try
                    {
                        var valueHeap = heap.Get(addr, dataToDelete);
                        throw new Exception("ERROR: Deleted value still retrievable!");
                    }
                    catch (Exception)
                    {

                    }
                    while (testDataList.Count > 0 && testDataList.Last().Count == 0)
                    {
                        testDataList.RemoveAt(testDataList.Count - 1);
                    }
                    if (testDataList.Count != heap.GetBlockCount())
                    {
                        throw new Exception("ERROR: Block count mismatch after deletion!");
                    }
                    int validCountAfterDelete;
                    try
                    {
                        validCountAfterDelete = heap.GetValidCountInBlock(addr);
                    }
                    catch (Exception)
                    {
                        validCountAfterDelete = 0;
                    }
                    if (validCountAfterDelete != validCountBeforeDelete - 1)
                    {
                        throw new Exception("ERROR: Valid count in block did not decrease after deletion!");
                    }
                }
                else
                {
                    Console.Write("Operation Get: ");
                    if (testDataList.Count == 0)
                    {
                        Console.WriteLine("No data to get.");
                        continue;
                    }
                    int addr = rand.Next(0, testDataList.Count);
                    if (testDataList[addr].Count == 0)
                    {
                        Console.WriteLine($"No data to get at block {addr}");
                        continue;
                    }
                    int dataIndex = rand.Next(0, testDataList[addr].Count);
                    Patient dataToGet = testDataList[addr][dataIndex];
                    var valueHeap = heap.Get(addr, dataToGet);
                    var valueList = testDataList[addr][dataIndex];
                    if (!valueList.Equals(valueHeap) && !dataToGet.Equals(valueHeap))
                    {
                        throw new Exception("ERROR: Retrieved value does not match expected value!");
                    }
                    Console.WriteLine($"Retrieved Patient {dataToGet.UniqueID} from address {addr}");
                }
                Console.WriteLine($"Current block count: {heap.GetBlockCount()}");
                Console.Write("Free blocks: ");
                foreach (var freeBlock in heap.GetFreeBlocks())
                {
                    Console.Write($"{freeBlock} ");
                }
                Console.WriteLine();
                Console.Write("Not Full Blocks: ");
                foreach (var notFullBlock in heap.GetNotFullBlocks())
                {
                    Console.Write($"{notFullBlock} ");
                }
                Console.WriteLine();
                for (int i = 0; i < heap.GetBlockCount(); i++)
                {
                    Console.WriteLine($"--- Block {i} ---");
                    var records = heap.GetAllRecordsInBlock(i);
                    for (int j = 0; j < records.Count; j++)
                    {
                        Console.WriteLine($"Value {j}: {records[j].UniqueID} {records[j].Name} {records[j].Surname} {records[j].BirthDate.Date}");
                    }
                }
                Console.WriteLine("-------------------");
                Console.WriteLine("For another random operation press any button, to exit press esc");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    heap.Close();
                    break;
                }
            }
            heap = new Heap<Patient>(recordSize, testFilePath, metaFilePath);
            for (int i = 0; i < testDataList.Count; i++)
            {
                var records = heap.GetAllRecordsInBlock(i);
                if (records.Count != testDataList[i].Count)
                {
                    throw new Exception("ERROR: Record count mismatch after reopening heap!");
                }
                for (int j = 0; j < records.Count; j++)
                {
                    if (!records[j].Equals(testDataList[i][j]))
                    {
                        throw new Exception("ERROR: Record data mismatch after reopening heap!");
                    }
                }
            }
            heap.Close();
            Console.Clear();
            Console.WriteLine("All tests passed successfuly");
        }
    }
}
