using System;

namespace MaximovInk.MKPlatformer
{
    [System.Serializable]
    public struct MKControllerState : IComparable, IEquatable<MKControllerState>
    {
        public bool Equals(MKControllerState other)
        {
            return Priority.Equals(other.Priority) && ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is MKControllerState other && Equals(other);
        }

        public float Priority;
        public string ID;

        public override int GetHashCode()
        {
            return HashCode.Combine(Priority, ID);
        }

        public int CompareTo(object obj)
        {
            return ID.CompareTo(obj);
        }

        public override string ToString()
        {
            return ID;
        }

        public static bool operator ==(MKControllerState c1, MKControllerState c2)
        {
            return c1.ID == c2.ID;
        }

        public static bool operator !=(MKControllerState c1, MKControllerState c2)
        {
            return c1.ID != c2.ID;
        }
    }

    public static class MKControllerStates
    {
        public static MKControllerState Idle = new()   { Priority = 0,  ID = "idle" };
        public static MKControllerState Walk = new()   { Priority = 1, ID = "walk" };
        public static MKControllerState Run = new()    { Priority = 1.5f, ID = "run" };
        public static MKControllerState Sprint = new() { Priority = 1.75f, ID = "sprint" };
        public static MKControllerState Fall = new() { Priority = 2, ID = "fall" };
        public static MKControllerState Crouch = new() { Priority = 3, ID = "crouch" };
        public static MKControllerState CrouchWalk = new() { Priority = 3.5f, ID = "crouchWalk" };

        public static MKControllerState WallClimb = new() { Priority = 4, ID = "wallClimb" };
        public static MKControllerState Ledge = new() { Priority = 4.5f, ID = "wallLedge" };

        public static MKControllerState Jump = new()   { Priority = 5,  ID = "jump" };
        public static MKControllerState WallJump = new()   { Priority = 5,  ID = "wallJump" };
        public static MKControllerState Slide = new() { Priority = 6, ID = "slide" };
        public static MKControllerState Dash = new()   { Priority = 7,  ID = "dash" };

        public static MKControllerState PushIdle = new() { Priority = 8, ID = "pushIdle" };
        public static MKControllerState PushMove = new() { Priority = 9, ID = "pushMove" };


        public static float GetPriority(MKControllerState state)
        {
            return state.Priority;
        }
    }

    //Enum example:
    /*
    public enum MKControllerState : int
     {
         Idle,
         Walk,
         Run,
         Sprint,
         Jump
     }

     public static class MKControllerStatesEnum
     {
         public static MKControllerState Idle = MKControllerState.Idle;

         public static MKControllerState Walk = MKControllerState.Walk;
         public static MKControllerState Run = MKControllerState.Run;
         public static MKControllerState Sprint = MKControllerState.Sprint;

         public static MKControllerState Jump = MKControllerState.Jump;

         public static int GetPriority(MKControllerState state)
         {
             return (int)state;
         }
     }*/

}
