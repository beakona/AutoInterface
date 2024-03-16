using System.Text;

namespace BeaKona.AutoInterfaceGenerator;

internal sealed class SourceBuilder
{
    public SourceBuilder(AliasRegistry aliasRegistry, TypeRegistry missingAttributesRegistry, SourceBuilderOptions options) : this(aliasRegistry, missingAttributesRegistry, options, true)
    {
    }

    private SourceBuilder(AliasRegistry aliasRegistry, TypeRegistry missingAttributesRegistry, SourceBuilderOptions options, bool isRoot)
    {
        this.AliasRegistry = aliasRegistry;
        this.MissingAttributesRegistry = missingAttributesRegistry;
        this.Options = options;
        this.IsRoot = isRoot;
    }

    public SourceBuilderOptions Options { get; }
    public AliasRegistry AliasRegistry { get; }
    public TypeRegistry MissingAttributesRegistry { get; }
    public bool IsRoot { get; }

    private readonly List<object> elements = [];

    public void Clear()
    {
        this.elements.Clear();
    }

    private bool aliasMarkerAdded = false;
    public void MarkPointForAliases()
    {
        if (this.IsRoot == false) throw new InvalidOperationException();
        if (this.aliasMarkerAdded) throw new InvalidOperationException();
        this.elements.Add(new AliasesMarker());
        this.aliasMarkerAdded = true;
    }

    public void AppendLine() => this.elements.Add(new LineSeparatorMarker());

    public void AppendLine(string? text)
    {
        if (text != null)
        {
            this.Append(text);
        }
        this.AppendLine();
    }

    public void AppendLine(char c)
    {
        this.Append(c);
        this.AppendLine();
    }

    public void Append(string text)
    {
        if (text != null)
        {
            this.elements.Add(text);
        }
    }

    public void Append(object? value)
    {
        if (value != null)
        {
            this.Append(value.ToString());
        }
    }

    public void Append(char c) => this.elements.Add(c);

    public void AppendSeparated(string text)
    {
        if (text != null)
        {
            this.AppendSpaceIfNecessary();
            this.Append(text);
        }
    }

    public void AppendSpaceIfNecessary()
    {
        this.elements.Add(new FlexibleSpaceMarker());
    }

    private int currentDepth = 0;

    public void IncrementIndentation() => this.currentDepth++;

    public void DecrementIndentation()
    {
        if (this.currentDepth > 0)
        {
            this.currentDepth--;
        }
    }

    public void AppendIndentation()
    {
        this.elements.Add(new IndentationMarker(this.currentDepth));
    }

    public SourceBuilder AppendNewBuilder(bool register = true)
    {
        var builder = new SourceBuilder(this.AliasRegistry, this.MissingAttributesRegistry, this.Options, false);
        if (register)
        {
            this.elements.Add(builder);
        }
        return builder;
    }

    public SourceBuilder AppendBuilder(Action<SourceBuilder> builder)
    {
        if (builder != null)
        {
            SourceBuilder sb = this.AppendNewBuilder();
            builder(sb);
        }
        return this;
    }

    public override string ToString()
    {
        var text = new StringBuilder();
        this.Write(text);
        return text.ToString();
    }

    private void WriteAliases(StringBuilder builder)
    {
        foreach (string alias in this.AliasRegistry)
        {
            builder.Append("extern alias ");
            builder.Append(alias);
            builder.AppendLine(";");
        }
    }

    private void WriteIndentation(StringBuilder builder, IndentationMarker indentation, Dictionary<int, string>? cache = null)
    {
        int depth = indentation.Depth;
        if (cache == null || cache.TryGetValue(depth, out string value) == false)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Append(this.Options.Indentation);
            }
            value = sb.ToString();
            if (cache != null)
            {
                cache[depth] = value;
            }
        }
        builder.Append(value);
    }

    private void WriteFlexibleSpace(StringBuilder builder, char previous)
    {
        switch (previous)
        {
            case ' ':
            case '\t':
            case '\r':
            case '\n':
            case '(':
            case '[':
            case '{':
            case ')':
            case ']':
            case '}':
            case ';':
                break;
            default:
                builder.Append(' ');
                break;
        }
    }

    private void Write(StringBuilder builder)
    {
        var cache = new Dictionary<int, string>();

        if (this.aliasMarkerAdded == false)
        {
            this.WriteAliases(builder);
        }

        foreach (object? element in this.elements)
        {
            if (element != null)
            {
                switch (element)
                {
                    default: throw new NotSupportedException(nameof(Write));
                    case string text:
                        builder.Append(text);
                        break;
                    case char ch:
                        builder.Append(ch);
                        break;
                    case AliasesMarker:
                        this.WriteAliases(builder);
                        break;
                    case LineSeparatorMarker:
                        builder.Append(this.Options.NewLine);
                        break;
                    case IndentationMarker indentation:
                        this.WriteIndentation(builder, indentation, cache);
                        break;
                    case FlexibleSpaceMarker:
                        if (builder.Length > 0)
                        {
                            char previous = builder[builder.Length - 1];
                            this.WriteFlexibleSpace(builder, previous);
                        }
                        break;
                    case SourceBuilder sb:
                        sb.Append(builder);
                        break;
                }
            }
        }
    }

    private sealed class AliasesMarker
    {
    }

    private sealed class LineSeparatorMarker
    {
    }

    private sealed class IndentationMarker(int depth)
    {
        public readonly int Depth = depth;
    }

    private sealed class FlexibleSpaceMarker
    {
    }
}
