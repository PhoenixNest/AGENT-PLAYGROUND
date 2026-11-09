using System.Text;

namespace Tabtale.TTPlugins.SimpleJSON
{
	public class JSONString : JSONNode
	{
		private string m_Data;

		public override JSONNodeType Tag => (JSONNodeType)0;

		public override bool IsString => false;

		public override string Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public override Enumerator GetEnumerator()
		{
			return default;
		}

		public JSONString(string aData)
		{
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
		}

		public override bool Equals(object obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}
