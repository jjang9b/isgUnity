#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("2MeV6+2qun6Ct7RpH9x6IKhxmty8Do2uvIGKhaYKxAp7gY2NjYmMj9w3zTC2Nb5gixkZlymPwJMdBt8MNPTTnC9hUoZgmFed8tJ/rvarUleRds89HLFb0Fv2EaDlwehmvbYGXw6Ng4y8Do2Gjg6NjYwjeYTE1TkjiKRQGkHf2ShEGoNFuNdZX2BslxXT5aK8s9Hgt4ljvE/FeXGnN4MF//tnNIIH2LsiMVQMJkqBaBkkZ1BFspsxmxtzYFBEr0S/LbyQjDTZZbabzxw9/BtRAM2TrVyERkiuqlKwoSyjmvBAZ5iZQZz2w2DzDpbZLX0Er14C1KdGeYCdsv2UJWrqnJxq6DR7xjfDtDxs5NQT6LJB1XP2WG8L/Fdab+trUUTv046PjYyN");
        private static int[] order = new int[] { 6,6,10,4,12,6,8,12,13,11,10,11,12,13,14 };
        private static int key = 140;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
