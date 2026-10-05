namespace MimicPartyAccess.Game.Screens;

/// <summary>Entry point: registers the season pass, shop and cosmetic readers with the UI profile.</summary>
public static class ScreenReaders
{
    public static void Register()
    {
        MimicUiProfile.NameReaders.Add(SeasonReaders.NameOf);
        MimicUiProfile.NameReaders.Add(ShopReaders.NameOf);
        MimicUiProfile.StateReaders.Add(SeasonReaders.StateOf);
        MimicUiProfile.StateReaders.Add(PriceTags.StateOf);
    }
}
