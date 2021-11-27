using System.Collections;
using System.Collections.Generic;
using Genetics.Genes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Genetics
{
    public class ChromosomeCreationTests
    {
        [Test]
        public void ChromosomeWithOneGeneTwoCopiesHasWellFormedData()
        {
            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;

            var singleGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            chromosome.genes = new GeneEditor[] { singleGene };

            var newGene = chromosome.GenerateChromosomeData(new System.Random(0));

            Assert.AreEqual(1, newGene.allGeneData[0].chromosomeData.Length);
            Assert.AreEqual(2, chromosome.ChromosomeGeneticSize().allelePosition);
            Assert.AreEqual(2, newGene.allGeneData.Length);
        }

        [Test]
        public void ChromosomeWithFiveGenesTwoCopiesHasWellFormedData()
        {
            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;

            var mendelSwitches = new MendelianBooleanSwitch[] {
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>()};
            mendelSwitches[1].originIndex = 2;
            mendelSwitches[2].originIndex = 4;
            mendelSwitches[3].originIndex = 6;
            mendelSwitches[4].originIndex = 8;

            chromosome.genes = new GeneEditor[] {
                mendelSwitches[0],
                mendelSwitches[1],
                mendelSwitches[2],
                mendelSwitches[3],
                mendelSwitches[4]};

            var newGene = chromosome.GenerateChromosomeData(new System.Random(0));

            //4 base pairs per byte
            Assert.AreEqual(3, newGene.allGeneData[0].chromosomeData.Length);
            Assert.AreEqual(10, chromosome.ChromosomeGeneticSize().allelePosition);
            Assert.AreEqual(2, newGene.allGeneData.Length);
        }

        [Test]
        public void ChromosomeWithWideOverlappingGenesTwoCopiesHasWellFormedData()
        {
            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;

            var mendelSwitches = new MendelianBooleanSwitch[] {
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>()};
            mendelSwitches[0].originIndex = 0;
            mendelSwitches[0].volatility = 5;
            mendelSwitches[1].originIndex = 2;
            mendelSwitches[1].volatility = 5;

            chromosome.genes = new GeneEditor[] {
                mendelSwitches[0],
                mendelSwitches[1]};

            var newGene = chromosome.GenerateChromosomeData(new System.Random(0));

            //4 base pairs per byte
            Assert.AreEqual(2, newGene.allGeneData[0].chromosomeData.Length);
            Assert.AreEqual(7, chromosome.ChromosomeGeneticSize().allelePosition);
            Assert.AreEqual(2, newGene.allGeneData.Length);
        }

    }
}