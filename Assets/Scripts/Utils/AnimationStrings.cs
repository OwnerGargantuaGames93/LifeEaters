
namespace Utils
{
  internal class AnimationStrings {
    // --- Existing (reused) ---
    internal static string isMoving = "isMoving";
    internal static string jump = "jump";                        // reserved, not wired to any transition yet
    internal static string isFalling = "isFalling";
    internal static string isGrounded = "isGrounded";
    internal static string isOnWall = "isOnWall";                 // reserved/debug
    internal static string isCeiling = "isCeiling";               // reserved/debug
    internal static string isCrouching = "isCrouching";

    // --- Movement / objects / prayer ---
    internal static string isDashing = "isDashing";
    internal static string isClimbing = "isClimbing";
    internal static string isWallSliding = "isWallSliding";
    internal static string isGoingUp = "isGoingUp";
    internal static string isCarryingObject = "isCarryingObject";
    internal static string isPraying = "isPraying";

    internal static string startStopPray = "startStopPray";       // trigger
    internal static string throwTrigger = "throwTrigger";         // trigger
    internal static string grabTrigger = "grabTrigger";           // trigger - pit object grab only
    internal static string slamTrigger = "slamTrigger";           // trigger, reserved, no caller yet
    internal static string hurtTrigger = "hurtTrigger";           // trigger, reserved, no clip yet

    internal static string runAnimSpeedMultiplier = "runAnimSpeedMultiplier";     // float
    internal static string climbAnimSpeedMultiplier = "climbAnimSpeedMultiplier"; // float

    // --- Death ---
    internal static string isDying = "isDying";                   // bool, gates the death loop (player + enemies)
  }
}
