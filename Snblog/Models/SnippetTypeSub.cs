﻿using System;
using System.Collections.Generic;

namespace Snblog.Models;

/// <summary>
/// 片段分类下的子类
/// </summary>
public partial class SnippetTypeSub
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int TypeId { get; set; }

    public virtual ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();

    public virtual SnippetType Type { get; set; }
}
