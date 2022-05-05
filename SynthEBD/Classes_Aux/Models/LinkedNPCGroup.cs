﻿using Mutagen.Bethesda.Plugins;

namespace SynthEBD;

public class LinkedNPCGroup
{
    public LinkedNPCGroup()
    {
        this.GroupName = "";
        this.NPCFormKeys = new HashSet<FormKey>();
        this.Primary = new FormKey();
    }

    public string GroupName { get; set; }
    public HashSet<FormKey> NPCFormKeys { get; set; }
    public FormKey Primary { get; set; }
}