namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Philip;

public sealed class CryingChild2 : CryingChild
{
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        attackingAlly = true;
    }
}