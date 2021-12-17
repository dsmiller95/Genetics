using System.Collections;
using System.Collections.Generic;
using Genetics.Genes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Genetics
{
    public class GeneticEditingToolsTests
    {
        [Test]
        public void SingleChromosomeCopySamplesAtIndex()
        {
            var singleChromosomeCopy = new SingleChromosomeCopy
            {
                chromosomeData = new byte[]
                {
                    0b00011011,
                    0b10011111,
                    0b01010110
                }
            };
            Assert.AreEqual(0, singleChromosomeCopy.SampleIndex(new GeneIndex(0)));
            Assert.AreEqual(1, singleChromosomeCopy.SampleIndex(new GeneIndex(1)));
            Assert.AreEqual(2, singleChromosomeCopy.SampleIndex(new GeneIndex(2)));
            Assert.AreEqual(3, singleChromosomeCopy.SampleIndex(new GeneIndex(3)));
            Assert.AreEqual(2, singleChromosomeCopy.SampleIndex(new GeneIndex(4)));
            Assert.AreEqual(1, singleChromosomeCopy.SampleIndex(new GeneIndex(5)));
            Assert.AreEqual(2, singleChromosomeCopy.SampleIndex(new GeneIndex(11)));
        }
        [Test]
        public void SingleChromosomeCopySetsAndSamplesAtIndex()
        {
            var data = new SingleChromosomeCopy
            {
                chromosomeData = new byte[]
                {
                    0b00011011,
                    0b10011111,
                    0b01010110
                }
            };
            Assert.AreEqual(1, data.SampleIndex(new GeneIndex(1)));
            data.SetBasePairAtIndex(new GeneIndex(1), 3);
            Assert.AreEqual(3, data.SampleIndex(new GeneIndex(1)));

            Assert.AreEqual(3, data.SampleIndex(new GeneIndex(7)));
            data.SetBasePairAtIndex(new GeneIndex(7), 1);
            Assert.AreEqual(1, data.SampleIndex(new GeneIndex(7)));
        }

        [Test]
        public void SingleChromosomeCopySamplesRange()
        {
            var singleChromosomeCopy = new SingleChromosomeCopy
            {
                chromosomeData = new byte[]
                {
                    0b11000111,
                    0b10011111,
                    0b01010110
                }
            };

            Assert.AreEqual(0b11000111, singleChromosomeCopy.SampleBasePairs(new GeneSpan()
            {
                start = new GeneIndex(0),
                end = new GeneIndex(4)
            }));
            Assert.AreEqual(0b01111001, singleChromosomeCopy.SampleBasePairs(new GeneSpan()
            {
                start = new GeneIndex(2),
                end = new GeneIndex(6)
            }));
            Assert.AreEqual(0b0111110101, singleChromosomeCopy.SampleBasePairs(new GeneSpan()
            {
                start = new GeneIndex(5),
                end = new GeneIndex(10)
            }));
        }

        [Test]
        public void SingleChromosomeCopyWritesGeneSpan()
        {
            var data = new SingleChromosomeCopy
            {
                chromosomeData = new byte[]
                {
                    0b00011011,
                    0b10011111,
                    0b11010110,
                    0b11000000,
                    0b00110100,
                }
            };

            var writtenData = new byte[]
            {
                0b11001100,
                0b01010101,
                0b10101010,
            };

            var writeSpan = new GeneSpan()
            {
                start = new GeneIndex(6),
                end = new GeneIndex(13)
            };

            var expectedData = new byte[]
            {
                0b00011011,
                0b10011100,
                0b01010101,
                0b10000000,
                0b00110100,
            };

            data.WriteIntoGeneSpan(writeSpan, writtenData);
            for (int i = 0; i < expectedData.Length; i++)
            {
                Assert.AreEqual(expectedData[i], data.chromosomeData[i], $"Expected {System.Convert.ToString(data.chromosomeData[i], 2)} to be {System.Convert.ToString(expectedData[i], 2)} at index {i}");
            }
        }
    }
}