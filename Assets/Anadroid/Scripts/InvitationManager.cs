using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;

/*
** A singleton class which handles any incoming invitations to play 
** from a friend
*/

public class InvitationManager
{
    // the singleton instance
    private static InvitationManager sInstance = new InvitationManager();

    // used to get reference to any active invitations
    private Invitation mInvitation = null;

    // does the player want to auto accept invitations
    private bool mShouldAutoAccept = false;

    // handle an invite
    public void OnInvitationReceived(Invitation inv, bool shouldAutoAccept)
    {
        mInvitation = inv;
        mShouldAutoAccept = shouldAutoAccept;
    }

    // decline an invite and clear current invite data
    public void DeclineInvitation()
    {
        if (mInvitation != null)
        {
            PlayGamesPlatform.Instance.RealTime.DeclineInvitation(mInvitation.InvitationId);
        }
        Clear();
    }

    // reset invite data
    public void Clear()
    {
        mInvitation = null;
        mShouldAutoAccept = false;
    }

    public static InvitationManager Instance
    {
        get
        {
            return sInstance;
        }
    }

    public Invitation Invitation
    {
        get
        {
            return mInvitation;
        }
    }

    public bool ShouldAutoAccept
    {
        get
        {
            return mShouldAutoAccept;
        }
    }
}
