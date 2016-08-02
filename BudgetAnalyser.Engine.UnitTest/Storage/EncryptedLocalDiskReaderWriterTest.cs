using System;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Storage
{
    [TestClass]
    public class EncryptedLocalDiskReaderWriterTest
    {
        private EncryptedLocalDiskReaderWriter subject;
        private Mock<IFileEncryptor> mockFileEncryptor;
        private Mock<ICredentialStore> mockCredentialStore;

        [TestInitialize]
        public void TestSetup()
        {
            this.mockFileEncryptor = new Mock<IFileEncryptor>();
            this.mockCredentialStore = new Mock<ICredentialStore>();
            this.subject = new EncryptedLocalDiskReaderWriter(this.mockFileEncryptor.Object, this.mockCredentialStore.Object);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenTilde()
        {
            var result = this.subject.IsValidAlphaNumericWithPunctuation("~~~~~~~~");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_Given1Char()
        {
            var result = this.subject.IsValidAlphaNumericWithPunctuation(" ");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenEmpty()
        {
            var result = this.subject.IsValidAlphaNumericWithPunctuation("");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenAnyNumber()
        {
            var number = Enumerable.Range(0, 10);
            var text = string.Concat(number);
            var result = this.subject.IsValidAlphaNumericWithPunctuation(text);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols1()
        {
            var text = "~`!@#$%^&*()_-+=";
            var result = this.subject.IsValidAlphaNumericWithPunctuation(text);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols2()
        {
            var text = @"\|]}{[:;""',<.>";
            var result = this.subject.IsValidAlphaNumericWithPunctuation(text);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols3()
        {
            var text = "/?";
            var result = this.subject.IsValidAlphaNumericWithPunctuation(text);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenLargeTextBlob()
        {
            var result = this.subject.IsValidAlphaNumericWithPunctuation("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer in massa ullamcorper, aliquam augue sed, vulputate risus. Morbi et sagittis massa. Nulla nec ante aliquam, suscipit eros a, efficitur ligula. Suspendisse ac arcu mattis, dignissim libero eget, euismod elit. Nunc bibendum rutrum efficitur. Ut quis velit non tortor convallis gravida. Ut sed velit feugiat augue aliquam eleifend. Nam nulla dui, sagittis nec congue lacinia, ornare eget odio. Donec sed augue ac est vestibulum porttitor. Maecenas porttitor ligula erat, et facilisis lacus rhoncus non. Nulla consequat massa lorem, sed pellentesque nunc facilisis ultricies. Quisque at malesuada magna. Proin consectetur justo vel ipsum ornare luctus. Ut sed sodales mauris.Fusce mattis cursus risus in venenatis.Donec mollis leo at mauris lacinia accumsan.Nunc suscipit, urna vitae laoreet posuere, nunc nulla scelerisque diam, sed aliquam turpis nulla nec mauris.Sed dictum eget arcu non suscipit.Pellentesque ut semper ipsum.Integer sem nisl, tempor varius sodales a, convallis in elit.Vestibulum gravida ac quam eu fringilla. Praesent tincidunt turpis augue, eu lobortis mi convallis quis.Vivamus finibus tincidunt dolor sit amet convallis.Integer molestie tortor nec euismod eleifend.Nam vitae purus fringilla, condimentum magna tristique, vulputate est.Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.Maecenas varius orci in nunc sagittis congue.Integer mattis venenatis velit, sed venenatis nibh volutpat et.Mauris aliquet urna nulla, at auctor augue hendrerit a.Vestibulum justo diam, efficitur non accumsan quis, imperdiet a sapien.Phasellus fermentum erat augue, id eleifend eros pharetra ac.Aliquam bibendum porttitor est, eu tincidunt felis commodo imperdiet.Phasellus lobortis quis lorem quis pulvinar. Cras vitae dignissim nibh.Aliquam auctor mauris non velit aliquet, vitae pharetra lectus vestibulum.Proin et tellus diam.Mauris in nisi eu urna feugiat convallis iaculis eu dui.Cras lobortis, justo at finibus tincidunt, ligula lacus ullamcorper libero, at vestibulum lorem enim at tortor.Sed varius sapien varius arcu suscipit, a aliquet lorem pharetra.Praesent mattis nibh id leo viverra, et placerat ipsum cursus.In massa lectus, fermentum quis commodo id, blandit nec quam.Quisque pharetra tempor urna quis hendrerit.Proin dolor urna, porttitor volutpat arcu accumsan, tempor tincidunt lectus.Duis tristique euismod turpis.Duis suscipit interdum nibh, sit amet rutrum tortor faucibus quis. Morbi tempor consequat risus quis consectetur.Nullam in augue magna.Cras id porttitor neque.Duis efficitur lectus et tincidunt egestas.Ut euismod mollis massa.Nam blandit quam sed porta semper.Etiam viverra cursus iaculis.Phasellus pretium odio nec arcu malesuada, eget ultrices nisl porta.");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeFalse_GivenNull()
        {
            var result = this.subject.IsValidAlphaNumericWithPunctuation(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidAlphaNumericWithPunctuation_ShouldBeFalse_GivenTextStartsWithNull()
        {
            var bytes = new byte[] { 0x00000000, 0x00000000 };
            char[] chars = Encoding.UTF8.GetChars(bytes);
            var result = this.subject.IsValidAlphaNumericWithPunctuation(new string(chars));
            Assert.IsFalse(result);
        }
    }
}
