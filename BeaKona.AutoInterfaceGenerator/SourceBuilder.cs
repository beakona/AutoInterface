﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        private SourceBuilder(SourceBuilder owner, SourceBuilderOptions options) : this(options)
        {
            this.owner = owner;
        }

        private readonly SourceBuilder? owner;

        public SourceBuilderOptions Options { get; }

        private readonly List<object> elements = new();
        private readonly HashSet<string> aliases = new();

        public void Clear()
        {
            this.elements.Clear();
        }

        public void RegisterAlias(string alias)
        {
            if (this.owner != null)
            {
                this.owner.RegisterAlias(alias);
            }
            else
            {
                this.aliases.Add(alias);
            }
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
                this.AppendSpaceIfNeccessary();
                this.Append(text);
            }
        }

        public void AppendSpaceIfNeccessary()
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
            SourceBuilder builder = new(this, this.Options);
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
            StringBuilder text = new();
            foreach (string alias in this.aliases.OrderByDescending(i => i))
            {
                text.Append("extern alias ");
                text.Append(alias);
                text.AppendLine(";");
            }
            this.Write(text);
            return text.ToString();
        }

        private void Write(StringBuilder builder)
        {
            Dictionary<int, string>? cache = null;
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
                                    StringBuilder sb = new();
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
