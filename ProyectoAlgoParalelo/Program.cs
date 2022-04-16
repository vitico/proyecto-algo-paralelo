using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoAlgoParalelo
{
    class TestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public int testNumber { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan TimePromedio { get; set; }
    }

    internal class Program
    {
        #region Variables Prueba

        // create a big array to test the performance of the algorithm
        private static readonly int[] testArray = Enumerable.Range(0, 10000).Reverse().ToArray();
        private static readonly int[] expectedArray = Enumerable.Range(0, 10000).ToArray();
        private static readonly int[] searchArray = Enumerable.Range(0, 10000000).ToArray();
        private const int numberOfTest = 100, numberOfSearch = 1000;

        #endregion

        #region Metodos que realizaran la prueba

        private static TestResult testSortAlgo(string name, Func<int[], int[]> algorithm)
        {
            // este metodo realiza N pruebas del algoritmo de ordenamiento para determinar el tiempo de ejecucion y el resultado
            bool isSorted = true;
            int i = 0;
            var timer = new Stopwatch();
            timer.Start();
            int[] lastResult = algorithm(testArray);
            for (i = 0; i < numberOfTest; i++)
            {
                lastResult = algorithm(testArray);
                if (lastResult.SequenceEqual(expectedArray)) continue;
                isSorted = false;
                break;
            }

            timer.Stop();

            return new TestResult()
            {
                Message = "[SearchTest \"" + name + "\"]: " + (isSorted ? "Success" : "Failed"),
                Name = name,
                Success = isSorted,
                Time = timer.Elapsed,
                TimePromedio = TimeSpan.FromMilliseconds(timer.Elapsed.TotalMilliseconds / numberOfTest),
                testNumber = i
            };
        }

        private static TestResult testSearchAlgo(string name, Func<int[], int, int> algorithm)
        {
            // este metodo realiza N pruebas del algoritmo de busqueda para determinar el tiempo de ejecucion y el resultado

            // time how long it takes to sort the array
            var isFound = true;
            int i;
            var timer = new Stopwatch();
            timer.Start();
            // get a random number to search for
            var searchNumber = searchArray[new Random().Next(0, searchArray.Length)];


            for (i = 0; i < numberOfSearch; i++)
            {
                if (algorithm(searchArray, searchNumber) == searchNumber) continue;
                isFound = false;
                break;
            }

            timer.Stop();

            return new TestResult()
            {
                Message = "[SortTest \"" + name + "\"]: " + (isFound ? "Success" : "Failed"),
                Success = isFound,
                Name = name,
                Time = timer.Elapsed,
                TimePromedio = TimeSpan.FromMilliseconds(timer.Elapsed.TotalMilliseconds / numberOfTest),
                testNumber = i
            };
        }

        #endregion

        private static int getMemoryUsage()
        {
            // get the current process
            var process = Process.GetCurrentProcess();
            // get the memory allocated to the process
            return (int) (process.PeakWorkingSet64 / 1024 / 1024);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Proceso".padString('-', Console.WindowWidth - 1, true, true));
            Console.WriteLine("Memoria Inicial: " + getMemoryUsage() + " MB");
            Task<TestResult>[] tasks = new Task<TestResult>[5];
            tasks[0] = Task.Run(() => testSortAlgo("BubbleSort", Algoritmos.BubbleSort));
            tasks[1] = Task.Run(() => testSortAlgo("InsertionSort", Algoritmos.InsertionSort));
            tasks[2] = Task.Run(() => testSortAlgo("QuickSort", Algoritmos.QuickSort));
            tasks[3] = Task.Run(() => testSearchAlgo("BinarySearch", Algoritmos.BinarySearch));
            tasks[4] = Task.Run(() => testSearchAlgo("SeqSearch", Algoritmos.SeqSearch));

            Console.WriteLine(
                "Esperando a que se completen los algoritmos".padString('-', Console.WindowWidth - 1, true, true));

            Task.WhenAll(tasks).Wait();

            Console.WriteLine("Mostrando resultados".padString('-', Console.WindowWidth, true, true));
            Console.WriteLine("Memoria Final: " + getMemoryUsage() + " MB");

            // print out the results in a table
            Utils.PrintTaskResult(tasks.Select(t => t.Result).ToArray());

            Console.WriteLine("Fin del programa".padString('-', Console.WindowWidth, true, true));
            Console.ReadKey();
        }
    }


    class Algoritmos
    {
        #region Algoritmos de ordenamiento

        public static int[] InsertionSort(int[] original)
        {
            //make a copy of the array
            var arr = original.copyArray();

            var n = arr.Length;
            for (var i = 1; i < n; ++i)
            {
                var key = arr[i];
                var j = i - 1;

                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j -= 1;
                }

                arr[j + 1] = key;
            }

            return arr;
        }


        private static int[] QuickSort(int[] arr, int low, int high)
        {
            if (low >= high) return arr;

            var pi = arr.Partition(low, high);

            QuickSort(arr, low, pi - 1);
            QuickSort(arr, pi + 1, high);

            return arr;
        }


        public static int[] QuickSort(int[] original)
        {
            // Este metodo es para no tener que pasarle todos los parametros al metodo "QuickSort"
            var arr = original.copyArray();
            return QuickSort(arr, 0, arr.Length - 1);
        }

        public static int[] BubbleSort(int[] original)
        {
            var arr = original.copyArray();
            var n = arr.Length;
            for (var i = 0; i < n - 1; i++)
            {
                for (var j = 0; j < n - i - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }

            return arr;
        }

        #endregion

        #region Algoritmos de busqueda

        public static int BinarySearch(int[] arr, int x)
        {
            var low = 0;
            var high = arr.Length - 1;

            while (low <= high)
            {
                var mid = (low + high) / 2;
                if (arr[mid] == x)
                    return mid;
                if (arr[mid] < x)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return -1;
        }

        public static int SeqSearch(int[] arr, int x)
        {
            var n = arr.Length;
            for (var i = 0; i < n; i++)
            {
                if (arr[i] == x)
                    return i;
            }

            return -1;
        }

        #endregion
    }

    static class Utils
    {
        /**
         * Este metodo es para realizar una copia del arreglo y evitar que se modifique el arreglo original
         */
        public static int[] copyArray(this int[] arr)
        {
            var n = arr.Length;
            var newArr = new int[n];
            for (var i = 0; i < n; i++)
                newArr[i] = arr[i];
            return newArr;
        }

        /**
         * este metodo es utilizado por "QuickSort" y divide el arreglo en dos partes, una parte con los elementos menores, y otra con los mayores
         * @param arr Arreglo a dividir
         * @param low Indice del primer elemento
         * @param high Indice del ultimo elemento
         */
        public static int Partition(this int[] arr, int low, int high)
        {
            var pivot = arr[high];
            var i = low - 1;

            for (var j = low; j < high; j++)
            {
                if (arr[j] > pivot) continue;
                i++;

                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);

            return i + 1;
        }

        #region Utilidades que no tienen que ver con los algoritmos

        public static void PrintTaskResult(TestResult[] tasks)
        {
            printTable("Resultados", new[]
                    {"Algoritmos", "Pruebas realizadas", "Tiempo (ms)", "Tiempo Promedio (ms)"},
                tasks.Select(t => new[]
                        {t.Name, t.testNumber.ToString(), t.Time.ToString(), t.TimePromedio.TotalMilliseconds.ToString()})
                    .GetEnumerator());
        }

        /**
         * Imprime una tabla utilizando las columnas y el enumerador con los valores
         * @param tableHeader Titulo de la tabla
         * @param columns Columnas de la tabla
         * @param values Valores de la tabla
         */
        private static void printTable(string tableHeader, string[] columns, IEnumerator<string[]> values)
        {
            // calcula el tamano maximo que puede tener alguna fila en base a los valores y los nombres de las columnas
            var columnsLength = new int[columns.Length];
            var data = new List<string[]>();
            while (values.MoveNext())
            {
                var current = values.Current;
                for (var i = 0; i < columns.Length; i++)
                {
                    if (current[i].Length > columnsLength[i])
                    {
                        columnsLength[i] = current[i].Length;
                    }
                }


                data.Add(current);
            }

            for (int i = 0; i < columns.Length; i++)
            {
                if (columnsLength[i] < columns[i].Length)
                    columnsLength[i] = columns[i].Length;
            }

            var totalLength =
                columnsLength.Sum() + 2 + columns.Length * 3; //este es el total de caracteres que puede tener la tabla
            // print the table header
            Console.WriteLine(tableHeader.padString('-', totalLength, true, true));
            // print the table header
            for (var i = 0; i < columns.Length; i++)
            {
                Console.Write(" | " + columns[i].padString(' ', columnsLength[i], true, false));
            }

            Console.WriteLine();
            Console.WriteLine("|".padString('-', totalLength, false, true));
            // print the table content, and pad the message string with spaces
            foreach (var row in data)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    Console.Write(" | ");
                    Console.Write(row[i].padString(' ', columnsLength[i], false));
                }

                Console.WriteLine(" | ");
            }

            Console.WriteLine("".padString('-', totalLength, false));
        }

        /**
         * Este metodo es para darle el formato al texto que se va a imprimir en la consola
         * @param s Cadena de texto a formatear
         * @param c Caracter que se va a utilizar para rellenar la cadena
         * @param length Longitud de la cadena
         * @param center Si se centra el texto o no
         * @param addSpace Si se agrega un espacio al inicio y al final de la cadena
         */
        public static string padString(this string s, char c, int length, bool center = false, bool addSpace = false)
        {
            if (addSpace)
                s = " " + s + " ";
            if (!center)
            {
                if (length <= s.Length)
                    return s;
                return (s + new string(c, length - s.Length));
            }

            var left = (length - s.Length) / 2;
            var right = length - s.Length - left;
            var leftString = left > 0 ? new string(c, left) : "";
            var rightString = right > 0 ? new string(c, right) : "";
            return leftString + s + rightString;
        }

        #endregion
    }
}