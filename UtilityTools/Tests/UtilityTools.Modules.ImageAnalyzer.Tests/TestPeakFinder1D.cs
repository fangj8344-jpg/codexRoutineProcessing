using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;

namespace UtilityTools.Modules.ImageAnalyzer.Tests
{
    internal class TestPeakFinder1D
    {
        [Test]
        public void TestPeakFind01()
        {
            var pf = new PeakFinder1D(new List<double>() { 1, 2, 3, 4, 1 });
            var peaks = pf.FindPeaks();
            Assert.That(peaks[0], Is.EqualTo(3));

            peaks = new PeakFinder1D(new List<double>() { 1, 2, 3, 4, 1, 3, 2 }).FindPeaks();
            Assert.That(peaks[1], Is.EqualTo(5));

            peaks = new PeakFinder1D(new List<double>() { 1, 3, 2, 1, 2, 3, 4, 3, 2, 1 }).FindPeaks(topk: 1);
            Assert.That(peaks[0], Is.EqualTo(6));
        }
    }
}
