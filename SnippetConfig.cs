using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Snippet
{
    [XmlRoot("snippets")]
    public class SnippetConfig
    {
        [XmlElement("category")]
        public List<SnippetCategory> SnippetCategories { get; set; }

        public static List<SnippetCategory> Load()
        {
            var s = new XmlSerializer(typeof(SnippetConfig));
            var currentDirectory = Path.GetDirectoryName(typeof(SnippetConfig).Assembly.Location);
            using var file = File.OpenRead(Path.Combine(currentDirectory, "snippets.xml"));
            var config = (SnippetConfig)s.Deserialize(file);
            return config.SnippetCategories;
        }
    }

    public class SnippetCategory
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("snippet")]
        public List<SnippetText> SnippetTexts { get; set; }

        public override string ToString()
        {
            return $" * {Name}";
        }
    }

    public class SnippetText
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Content { get; set; }

        public override string ToString()
        {
            return $" * {Name}";
        }
    }
}