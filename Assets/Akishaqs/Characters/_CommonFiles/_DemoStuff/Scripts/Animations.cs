using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animations : MonoBehaviour
{
    Animator _anim;
    public Text _animClipName;
    public GameObject _player;
    string _currentClipName;
    AnimatorClipInfo[] _currentClipInfo;
    int isCrouchingHash;
    int isRunningHash;
    int isWalkingHash;
    int isWalkingLeftHash;
    int isWalkingRightHash;
    int isReturningHash;
    int isRunningLeftHash;
    int isRunningRightHash;
    int isRunTurningHash;

    void Start()
    {
        _anim = _player.GetComponent<Animator>();
        isCrouchingHash = Animator.StringToHash("ToCrouch");
        isRunningHash = Animator.StringToHash("isRunning");
        isWalkingHash = Animator.StringToHash("isWalking");
        isWalkingLeftHash = Animator.StringToHash("isWalkingLeft");
        isWalkingRightHash = Animator.StringToHash("isWalkingRight");
        isReturningHash = Animator.StringToHash("isReturning");
        isRunningLeftHash = Animator.StringToHash("isRunningLeft");
        isRunningRightHash = Animator.StringToHash("isRunningRight");
        isRunTurningHash = Animator.StringToHash("isRunTurning");
    }

    /*
       public void next()
       {
           _anim.SetTrigger("ToNext");
       }
       public void previous()
       {
           _anim.SetTrigger("ToPrevious");
       }

       void OnGUI()
       {
           _currentClipInfo = _anim.GetCurrentAnimatorClipInfo(1);
           _currentClipName = _currentClipInfo[0].clip.name;
           _animClipName.text = _currentClipName;
       }
       */
    void Update()
    {
        bool isCrouch = _anim.GetBool(isCrouchingHash);
        bool CPress = Input.GetKeyDown(KeyCode.C);

        bool isRunning = _anim.GetBool(isRunningHash);
        bool ShiftPress = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        bool isWalking = _anim.GetBool(isWalkingHash);
        bool WPress = Input.GetKey(KeyCode.W);

        bool isWalkingLeft = _anim.GetBool(isWalkingLeftHash);
        bool APress = Input.GetKey(KeyCode.A);

        bool isWalkingRight = _anim.GetBool(isWalkingRightHash);
        bool DPress = Input.GetKey(KeyCode.D);

        bool isReturning = _anim.GetBool(isReturningHash);
        bool SPress = Input.GetKey(KeyCode.S);

        bool isRunningLeft = _anim.GetBool(isRunningLeftHash);
        bool isRunningRight = _anim.GetBool(isRunningRightHash);
        bool isRunTurning = _anim.GetBool(isRunTurningHash);

        if (!isCrouch && CPress)  // Nhấn phím C để cúi người
        {
            _anim.SetBool(isCrouchingHash, true);
        }
        if (isCrouch && CPress)
        {
            _anim.SetBool(isCrouchingHash, false);
        }

        if (!isRunning && ShiftPress && WPress)
        {
            _anim.SetBool(isRunningHash, true);
        }
        if (isRunning && !ShiftPress && !WPress)
        {
            _anim.SetBool(isRunningHash, false);
        }

        if (!isWalking && WPress && !ShiftPress)
        {
            _anim.SetBool(isWalkingHash, true);
        }
        if (isWalking && !WPress && !ShiftPress)
        {
            _anim.SetBool(isWalkingHash, false);
        }

        if (!isWalkingLeft && APress && !ShiftPress)
        {
            _anim.SetBool(isWalkingLeftHash, true);
        }
        if (isWalkingLeft && !APress && !ShiftPress)
        {
            _anim.SetBool(isWalkingLeftHash, false);
        }

        if (!isWalkingRight && DPress && !ShiftPress)
        {
            _anim.SetBool(isWalkingRightHash, true);
        }
        if (isWalkingRight && !DPress && !ShiftPress)
        {
            _anim.SetBool(isWalkingRightHash, false);
        }

        if (!isReturning && SPress && !ShiftPress)
        {
            _anim.SetBool(isReturningHash, true);
        }
        if (isReturning && !SPress && !ShiftPress)
        {
            _anim.SetBool(isReturningHash, false);
        }

        if (!isRunningLeft && APress && ShiftPress)
        {
            _anim.SetBool(isRunningLeftHash, true);
        }
        if (isRunningLeft && !APress && !ShiftPress)
        {
            _anim.SetBool(isRunningLeftHash, false);
        }

        if (!isRunningRight && DPress && ShiftPress)
        {
            _anim.SetBool(isRunningRightHash, true);
        }
        if (isRunningRight && !DPress && !ShiftPress)
        {
            _anim.SetBool(isRunningRightHash, false);
        }
        
        if (!isRunTurning && SPress && ShiftPress)
        {
            _anim.SetBool(isRunTurningHash, true);
        }
        if (isRunTurning && !SPress && !ShiftPress)
        {
            _anim.SetBool(isRunTurningHash, false);
        }


        
    }
}
