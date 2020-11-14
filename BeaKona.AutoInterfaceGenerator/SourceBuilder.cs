using System;
using System.Collections.Generic;
using System.Text;

namespace BeaKona.AutoInterfaceGenerator
{
    internal sealed class SourceBuilder
    {
        public SourceBuilder() : this(SourceBuilderOptions.Default)
        {
        }

        public SourceBuilder(SourceBuilderOptions options)
        {
            this.Options = options;
        }

        public SourceBuilderOptions Options { get; }

        private readonly List<object> items = new List<object>();

        public void Clear()
        {
            this.items.Clear();
        }

        public void AppendLine() => this.items.Add(new LineSeparatorMarker());

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
                this.items.Add(text);
            }
        }

        public void Append(object? value)
        {
            if (value != null)
            {
                this.Append(value.ToString());
            }
        }

        public void Append(char c) => this.items.Add(c);

        public void AppendSeparated(string text)
        {
            if (text != null)
            {
                this.AppendSpaceIfNeccessary();
                this.Append(text);
            }
        }

        public void AppendSpaceIfNeccessary()
        {
            this.items.Add(new FlexibleSpaceMarker());
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
            this.items.Add(new IndentationMarker(this.currentDepth));
        }

        public SourceBuilder AppendNewBuilder()
        {
            SourceBuilder builder = new SourceBuilder(this.Options);
            this.items.Add(builder);
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
            StringBuilder text = new StringBuilder();
            this.Write(text);
            return text.ToString();
        }

        private void Write(StringBuilder builder)
        {
            Dictionary<int, string>? cache = null;
            foreach (object? item in this.items)
            {
                if (item != null)
                {
                    switch (item)
                    {
                        default: throw new NotSupportedException();
                        case string text:
                            builder.Append(text);
                            break;
                        case char ch:
                            builder.Append(ch);
                            break;
                        case LineSeparatorMarker:
                            builder.Append(this.Options.NewLine);
                            break;
                        case IndentationMarker indentation:
                            {
                                if (cache == null)
                                {
                                    cache = new Dictionary<int, string>();
                                }
                                int depth = indentation.Depth;
                                if (cache.TryGetValue(depth, out string value) == false)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    for (int i = 0; i < depth; i++)
                                    {
                                        sb.Append(this.Options.Identation);
                                    }
                                    cache[depth] = value = sb.ToString();
                                }
                                builder.Append(value);
                                break;
                            }
                        case FlexibleSpaceMarker:
                            {
                                if (builder.Length > 0)
                                {
                                    char c = builder[builder.Length - 1];
                                    switch (c)
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

                                break;
                            }
                        case SourceBuilder sb:
                            sb.Append(builder);
                            break;
                    }
                }
            }
        }

        private class LineSeparatorMarker
        {
        }

        private class IndentationMarker
        {
            public IndentationMarker(int depth)
            {
                this.Depth = depth;
            }

            public readonly int Depth;
        }

        private class FlexibleSpaceMarker
        {
        }
    }
}
