namespace CosmicWinForms
{
    internal sealed class ScriptDocument
    {
        public ScriptDocument(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; set; }

        public string Content { get; set; }
    }
}
