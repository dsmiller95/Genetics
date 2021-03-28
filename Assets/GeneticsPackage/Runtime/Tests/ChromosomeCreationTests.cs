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

            Assert.AreEqual(1, newGene.allGeneData.Length);
            Assert.AreEqual(2, newGene.allGeneData[0].chromosomalCopies.Length);
        }

        [Test]
        public void ChromosomeWithThreeGenesTwoCopiesHasWellFormedData()
        {
            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;

            chromosome.genes = new GeneEditor[] {
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>(),
            ScriptableObject.CreateInstance<MendelianBooleanSwitch>()};

            var newGene = chromosome.GenerateChromosomeData(new System.Random(0));

            Assert.AreEqual(3, newGene.allGeneData.Length);
            Assert.AreEqual(2, newGene.allGeneData[0].chromosomalCopies.Length);
        }

    }
}