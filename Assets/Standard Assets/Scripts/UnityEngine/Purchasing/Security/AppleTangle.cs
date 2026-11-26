using System;

namespace UnityEngine.Purchasing.Security
{
	public class AppleTangle
	{
		private static byte[] data = Convert.FromBase64String("HE1bbTNtIWXL7I+O5Oa3KMK5ICi4G0sPj0J/VC6TondZdqLCC2E3zfNh8aaBMxSNf9NaSHqQYEaAKHGrTUpJTEhLTiJvdUtNSEpIQUpJTEh/lAVB+/MrWKtAvMnH4jdyE4dThM9jxes6XGpSv3dlzjXkJhuwM/hvSPp8w0j6e9vYe3p5enp5ekh1fnEHOdDggamyHuRcE2mo28OcY1K7Z3BTfnl9fX96eW5mEAwMCAtCV1cPd+VFi1MxUGKwhrbNwXahJmSus0VwJkj6eWl+ey1lWHz6eXBI+nl8SFRYGx0KDBEeERsZDB1YCBcUERsB7eYCdNw/8yOsbk9Ls7x3NbZsEakPD1YZCAgUHVYbFxVXGQgIFB0bGQwQFwoRDAFJbkhsfnstfHtrdTkIAkj6eQ5Idn57LWV3eXmHfHx7enk9Bmc0EyjuOfG8DBpzaPs5/0vy+Vyak6nPCKd3PZlfsokVAJWfzW9vzULVjHd2eOpzyVluVgytRHWjGm7GjAvjlqocd7MBN0yg2kaBAIcTsKFOB7n/LaHf4cFKOoOgrQnmBtkq0KQGWk2yXa2hd64TrNpcW2mP2dQRHhEbGQwRFxZYOQ0MEBcKEQwBSU7hNFUAz5X046SLD+OKDqoPSDe5S04iSBpJc0hxfnstfH5rei0rSWteSFx+ey18c2tlOQgIFB1YOx0KDGf9+/1j4UU/T4rR4zj2VKzJ6GqgCBQdWDsdCgwRHhEbGQwRFxZYOQ1+SHd+ey1la3l5h3x9SHt5eYdIZR/3cMxYj7PUVFgXCM5HeUj0zzu3AVgZCwsNFR0LWBkbGx0IDBkWGx0aFB1YCwwZFhwZChxYDB0KFQtYGW5IbH57LXx7a3U5CAgUHVgqFxcM+nl4fnFS/jD+jxscfXlI+YpIUn7JSCCUInxK9BDL92WmHQuHHyYdxFL+MP6PdXl5fX14SBpJc0hxfnstId99cQRvOC5pZgyrz/NbQz/brRd9eHv6eXd4SPp5cnr6eXl4nOnRcQoZGwwRGx1YCwwZDB0VHRYMC1ZI9wv5GL5jI3FX6sqAPDCIGEDmbY0xoA7nS2wd2Q/ssVV6e3l4edv6eVgZFhxYGx0KDBEeERsZDBEXFlgIWBceWAwQHVgMEB0WWBkICBQRGxkMER4RGxkMHVgaAVgZFgFYCBkKDEhpfnstfHJrcjkICBQdWDEWG1ZJ+GxTqBE/7A5xhowT9VY43o8/NQcIFB1YKhcXDFg7OUhmb3VITkhMSkVeH1jySxKPdfq3ppPbV4ErEiMcfnstZXZ8bnxsU6gRP+wOcYaME/XT2wnqPystuddXOcuAg5sItZ7bNFY43o8/NQdwJkhnfnstZVt8YEhusWEKjSV2rQcn44pde8It9zUldYln6aNmPyiTfZUmAfxVk07aLzQtlHx+a3otK0lrSGl+ey18cmtyOQgIWDs5SPp5Wkh1fnFS/jD+j3V5eXlXSPm7fnBTfnl9fX96ekj5zmL5yxQdWDEWG1ZJXkhcfnstfHNrZTkIdX5xUv4w/o91eXl9fXh7+nl5eCQqHRQRGRYbHVgXFlgMEBELWBsdChYcWBsXFhwRDBEXFgtYFx5YDQsdKNLyraKchKhxf0/IDQ1Z");

		private static int[] order = new int[61]
		{
			20,
			57,
			16,
			7,
			51,
			59,
			20,
			17,
			35,
			22,
			33,
			44,
			18,
			36,
			59,
			28,
			25,
			53,
			23,
			50,
			58,
			54,
			44,
			48,
			35,
			33,
			49,
			51,
			29,
			54,
			40,
			42,
			48,
			34,
			49,
			57,
			52,
			59,
			46,
			55,
			51,
			45,
			45,
			55,
			44,
			48,
			58,
			56,
			51,
			57,
			51,
			51,
			54,
			57,
			54,
			59,
			57,
			57,
			58,
			59,
			60
		};

		private static int key = 120;

		public static readonly bool IsPopulated = true;

		public static byte[] Data()
		{
			if (!IsPopulated)
			{
				return null;
			}
			return Obfuscator.DeObfuscate(data, order, key);
		}
	}
}
