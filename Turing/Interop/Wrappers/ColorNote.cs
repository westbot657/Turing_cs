
using System.Runtime.InteropServices;

namespace Turing.Interop.Wrappers
{
    [RustClass]
    public partial class ColorNote
    {
        [RustWrapped("colornote")]
        private NoteController idkIfThisIsTheRightObject;
        
        [RustMethod("colornote_set_position")]
        private void SetPosition(UnityEngine.Vector3 position, int t)
        {
            idkIfThisIsTheRightObject.beatPos.Set(position.x, position.y, position.z);
        }

        [RustMethod("colornote_set_orientation")]
        private void SetOrientation(UnityEngine.Quaternion orientation)
        {
            idkIfThisIsTheRightObject.worldRotation.Set(orientation.x, orientation.y, orientation.z, orientation.w);
        }

        [RustMethod("colornote_get_position")]
        private UnityEngine.Vector3 GetPosition()
        {
            return idkIfThisIsTheRightObject.beatPos;
        }

    }
}