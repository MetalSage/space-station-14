﻿using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Administration.UI.BanList.Bans;

[GenerateTypedNameReferences]
public sealed partial class BanListHeader : ContainerButton
{
    public BanListHeader()
    {
        RobustXamlLoader.Load(this);
    }
}
