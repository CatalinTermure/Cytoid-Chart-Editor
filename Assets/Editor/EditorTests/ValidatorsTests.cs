using CCE.Data.Validators;
using CCE.Utils;
using NUnit.Framework;

namespace CCE.EditorTests
{
    public class ValidatorsTests
    {
        [Test]
        public void LocalizedStringValidator_RejectsNonEnglishCharacters()
        {
            var validator = new LocalizedStringValidator();
            var results = validator.Validate("你好");
            Assert.Contains(ValidationResult.Error("Localized strings must only contain English letters, numbers, symbols, and spaces."), results);
        }

        [Test]
        public void LocalizedStringValidator_AllowsNumbers()
        {
            var validator = new LocalizedStringValidator();
            var results = validator.Validate("123");
            Assert.IsEmpty(results);
        }

        [Test]
        public void LocalizedStringValidator_AllowsEnglishLetters()
        {
            var validator = new LocalizedStringValidator();
            var results = validator.Validate("HelloWorld");
            Assert.IsEmpty(results);
        }

        [Test]
        public void LocalizedStringValidator_AllowsSymbolsAndSpaces()
        {
            var validator = new LocalizedStringValidator();
            var results = validator.Validate("!@#$%^&*()_+-=[]{}\\|;:'\",.<>/? ");
            Assert.IsEmpty(results);
        }

        [Test]
        public void LocalizedStringValidator_AllowsMixedValidCharacters()
        {
            var validator = new LocalizedStringValidator();
            var results = validator.Validate("Hello World 123!@#");
            Assert.IsEmpty(results);
        }

        [Test]
        public void LevelIdValidator_RejectsNonLowercaseCharacters()
        {
            var validator = new LevelIdValidator();
            var results = validator.Validate("Hello World");
            Assert.Contains(ValidationResult.Error("Level ID must contain only lowercase characters, digits, dots and underscores"), results);
        }

        [Test]
        public void LevelIdValidator_AllowsValidLevelId()
        {
            var validator = new LevelIdValidator();
            var results = validator.Validate("hello_world.level_name");
            Assert.IsEmpty(results);
        }

        [Test]
        public void LevelIdValidator_WarnsImproperFormat()
        {
            var validator = new LevelIdValidator();
            var results = validator.Validate("level_name_only_without_charter_name");
            Assert.Contains(ValidationResult.Warning("Level ID should be charter_name.level_name"), results);
        }

        [Test]
        public void SourceValidator_RejectsNonLinkSources()
        {
            var validator = new SourceValidator();
            var results = validator.Validate("Hello World");
            Assert.Contains(ValidationResult.Error("Source must be a link"), results);
        }

        [Test]
        public void SourceValidator_AllowsValidSources()
        {
            var validator = new SourceValidator();
            var results = validator.Validate("https://www.pixiv.com");
            Assert.IsEmpty(results);
        }
    }
}