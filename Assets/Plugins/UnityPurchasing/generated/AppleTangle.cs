#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("SCspWOppSlhlbmFC7iDun2VpaWkBDgELCRwBBwZIKR0cAAcaARwRWRgEDUg6BwccSCspWHZ/ZVheWFxa6mlobmFC7iDunwsMbWlY6ZpYQm6oC1sfn1JvRD6DsmdJZrLSG3En3UgHDkgcAA1IHAANBkgJGBgEAQsJBgxICwcGDAEcAQcGG0gHDkgdGw3ZWDCEMmxa5ADb53W2DRuXDzYN1CGwHvdbfA3JH/yhRWpraWhpy+pp1pwb84a6DGejESdcsMpWkRCXA6BYeW5rPWxie2IpGBgEDUghBgtGWS0WdyQDOP4p4awcCmN46ynvW+LpYDZY6ml5bms9dUhs6mlgWOppbFj99hJkzC/jM7x+X1ujrGclpnwBuV1aWVxYW14yf2VbXVhaWFFaWVxYHx9GCRgYBA1GCwcFRwkYGAQNCwkaCQscAQsNSBscCRwNBQ0GHBtGWGVuYULuIO6fZWlpbW1oa+ppaWg0d/mzdi84g22FNhHsRYNeyj8kPYRGKM6fLyUXYDZYd25rPXVLbHBYfuNx4baRIwSdb8NKWGqAcFaQOGG7FynA8JG5og70TAN5uMvTjHNCq3dvhBVR6+M7SLtQrNnX8idiA5dDlEdY6atuYENuaW1tb2pqWOnecunbD+dg3Eifo8RESAcY3ldpWOTfK6exXhep7z2xz/HRWiqTsL0Z9hbJOkyKg7nfGLdnLYlPopkFEIWP3X9/Mc9tYRR/KD55dhy73+NLUy/LvQdICQYMSAsNGhwBDgELCRwBBwZIGERICw0aHAEOAQsJHA1IGAcEAQsRoXEanTVmvRc385pNa9I95yU1ZZln9VWbQyFAcqCWpt3RZrE2dL6jVWxue2o9O1l7WHluaz1sYntiKRgY5xvpCK5zM2FH+tqQLCCYCFD2fZ1C7iDun2VpaW1taFgKWWNYYW5rPRwBDgELCRwNSAoRSAkGEUgYCRoc33PV+ypMekKvZ3XeJfQ2C6Aj6H8YBA1IKw0aHAEOAQsJHAEHBkgpHQxdS30jfTF12/yfnvT2pzjSqTA4BA1IIQYLRllOWExuaz1sY3t1KRgRSAkbGx0FDRtICQsLDRgcCQYLDcPLGfovOz2px0cp25CTixiljsskXvEkRRDfheTztJsf85oeuh9YJ6lVTg9I4lsCn2Xqp7aDy0eROwIzDG5YZ25rPXV7aWmXbG1Ya2lpl1h1YENuaW1tb2ppfnYAHBwYG1JHRx9taGvqaWdoWOppYmrqaWlojPnBYRwABxoBHBFZflh8bms9bGt7ZSkYTlhMbms9bGN7dSkYGAQNSCsNGhzdUsWcZ2Zo+mPZSX5GHL1UZbMKfljqbNNY6mvLyGtqaWpqaWpYZW5hCgQNSBscCQYMCRoMSBwNGgUbSAl+WHxuaz1sa3tlKRgYBA1IOgcHHBJY6mkeWGZuaz11Z2lpl2xsa2pp6HxDuAEv/B5hlpwD5UYozp8vJRd37evtc/FVL1+awfMo5kS82fh6sG5rPXVmbH5sfEO4AS/8HmGWnAPlW14yWApZY1hhbms9bG57aj07WXvAtBZKXaJNvbFnvgO8ykxLeZ/JxDoNBAEJBgsNSAcGSBwAARtICw0aOMLivbKMlLhhb1/YHR1J");
        private static int[] order = new int[] { 13,11,6,34,57,41,43,26,27,50,50,25,33,18,26,36,46,47,36,31,46,47,51,37,47,58,53,52,45,44,31,51,43,55,58,39,59,57,46,58,40,47,48,56,44,58,53,47,50,57,59,59,55,58,56,57,59,57,59,59,60 };
        private static int key = 104;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
