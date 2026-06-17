// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ZZm2YprsCffoqNlGYmnkZb0wYWjJSkRLe8lKQUnJSkpL/CfZrpQEFNBiG50yWo5SKHFmcbjHn/Ew2kKGlbBBGzrflXsAbtyVaATLdQvFLgLHeZjLskoN7+vtopJUhqrVdC9vvflXGg34Cl3cVaTfefPVBtCIIDWEe8lKaXtGTUJhzQPNvEZKSkpOS0hju0iE+MGyhW04WMouF7F8J9izuDX7YC/6EiElm9YWaDizyzrA3kwhQIfn39/4urDwqxQ75RTOyCer2PH5gL70PrfSQdCcLJABGQ4ZFG2FgN7qr3Ny0mNTlFzjJ0l7OlACqHYghmmG9nrw34659BwwMApKwwjsyWa6ySt+YOzTtbjVMJgyXgNRYbyqKlUP42JyA/RteElISktK");
        private static int[] order = new int[] { 6,1,11,9,13,9,6,11,10,9,10,12,12,13,14 };
        private static int key = 75;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
