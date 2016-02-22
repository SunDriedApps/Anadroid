using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;

public class InvitationManager
{
    private static InvitationManager sInstance = new InvitationManager();

    public static InvitationManager Instance
    {
        get
        {
            return sInstance;
        }
    }

    private Invitation mInvitation = null;
    private bool mShouldAutoAccept = false;

    public void OnInvitationReceived(Invitation inv, bool shouldAutoAccept)
    {
        mInvitation = inv;
        mShouldAutoAccept = shouldAutoAccept;
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

    public void DeclineInvitation()
    {
        if (mInvitation != null)
        {
            PlayGamesPlatform.Instance.RealTime.DeclineInvitation(mInvitation.InvitationId);
        }
        Clear();
    }

    public void Clear()
    {
        mInvitation = null;
        mShouldAutoAccept = false;
    }
}
