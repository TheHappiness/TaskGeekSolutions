﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static ConsoleColor currentColor;

        private static readonly object locker = new object();

        static void Main(string[] args)
        {
            currentColor = Console.ForegroundColor;
            double[] _bagMass = null;
            BagsInitialize(100000, 1500, ref _bagMass);

            do
            {
                try
                {
                    Console.Clear();
                    Console.ForegroundColor = currentColor;

                    Console.WriteLine("Enter N kg of sugar:");
                    double n = double.Parse(Console.ReadLine());

                    Console.WriteLine("\nThe indexes:\n");

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    foreach (var item in BuySuagarBags(n, _bagMass))
                    {
                        Console.Write($"[{item}] ");
                    }
                    sw.Stop();
                    Console.WriteLine("\nTIME: {0}", sw.ElapsedMilliseconds.ToString());
                    sw.Reset();

                    //PrintBugsMass(_bagMass);
                    Console.WriteLine("\n.....................\n\nThe indexes by parallel\n");

                    sw.Start();
                    foreach (var item in BuySuagarBagsByParallel(n, _bagMass))
                    {
                        Console.Write($"[{item}] ");
                    }
                    sw.Stop();
                    Console.WriteLine("\nTIME: {0}", sw.ElapsedMilliseconds.ToString());
                    sw.Reset();
                }
                catch (FormatException)
                {
                    SetRedFColor();
                    Console.WriteLine("Please, enter a correct data (digits)");
                }
                catch (Exception)
                {
                    SetRedFColor();
                    Console.WriteLine("An error ocurred. Please, enter a data (digits) again");
                }

                UserActions();


            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// To buy a fit bags by parallel;
        /// </summary>
        /// <param name="n"></param>
        /// <param name="_bagMass"></param>
        /// <returns></returns>
        private static IEnumerable<double> BuySuagarBagsByParallel(double n, double[] _bagMass)
        {
            if (_bagMass == null)
            {
                throw new ArgumentNullException("The array can not be null.");
            }

            ConcurrentStack<double> results = new ConcurrentStack<double>();
            bool isSuccess = false;
            try
            {
                Parallel.For(0, _bagMass.Length, (i, loopState) =>
                {
                    for (int j = i + 1; j < _bagMass.Length; j++)
                    {
                        if (_bagMass[i] + _bagMass[j] == n && isSuccess == false)
                        {
                            lock (locker)
                            {
                                isSuccess = true;
                            }
                            results.Push(i);
                            results.Push(j);
                            loopState.Stop();
                            return;
                        }
                    }

                    if (loopState.IsStopped)
                    {
                        return;
                    }
                });
            }
            catch (AggregateException e)
            {
                Console.WriteLine("An iteration has thrown an exception. THIS WAS NOT EXPECTED.\n{0}", e);
            }

            if (!isSuccess)
            {
                Console.WriteLine("No matching bags");
            }

            return results;
        }

        /// <summary>
        /// To buy a fit bags.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="bagMass"></param>
        /// <returns></returns>
        private static IEnumerable<double> BuySuagarBags(double n, double[] bagMass)
        {
            if (bagMass == null)
            {
                throw new ArgumentNullException("The array can not be null");
            }

            bool isSuccess = false;

            for (int i = 0; i < bagMass.Length; i++)
            {
                if (isSuccess) break;

                for (int j = i + 1; j < bagMass.Length; j++)
                {
                    if (bagMass[i] + bagMass[j] == n)
                    {
                        isSuccess = true;
                        yield return i;
                        yield return j;
                        break;
                    }
                }
            }

            if (!isSuccess)
            {
                Console.WriteLine("No matching bags");
            }
        }

        /// <summary>
        /// To print the array of mass.
        /// </summary>
        /// <param name="mass"></param>
        private static void PrintBugsMass(double[] mass)
        {
            if (mass == null)
            {
                throw new ArgumentNullException("The array can not be null");
            }

            Console.WriteLine("\n\nThe all array:");

            for (int i = 0; i < mass.Length; i++)
            {
                Console.Write($"i:{i} v:{mass[i]}\t\t");
            }
        }

        /// <summary>
        /// To initializes the array to a random weight.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="maxkg"></param>
        /// <param name="bagMass"></param>
        private static void BagsInitialize(int length, int maxkg, ref double[] bagMass)
        {
            Random random = new Random();
            bagMass = new double[length];

            for (int i = 0; i < length; i++)
            {
                bagMass[i] = random.Next(1, maxkg);
            }
        }

        private static void UserActions()
        {
            Console.ForegroundColor = currentColor;
            Console.WriteLine("\n\nEXIT => 'Esc'/ AGAIN => 'ENTER'");
        }

        private static void SetRedFColor()
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
    }
}
