//------------------------------------------------------------------------------
//  WARNING--WARNING--THIS IS GENERATED CODE; DO NOT EDIT!!--WARNING--WARNING
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
namespace Server.LOS
{
	//------------------------------------------------------------------------------
	public partial class LineOfSight
	{
		//------------------------------------------------------------------------------
		//  LOS shadow mask for visibility fields 31 cells square.
		//  Note: the individual visibility cells are bits in each byte.
		//------------------------------------------------------------------------------
		internal static ShadowMaker[,] /*[31,31]*/ m_ShadowMakers =
		{
  {
//  for an occlusion at [0,0]:
    null,
//  for an occlusion at [0,1]:
    null,
//  for an occlusion at [0,2]:
    null,
//  for an occlusion at [0,3]:
    null,
//  for an occlusion at [0,4]:
    null,
//  for an occlusion at [0,5]:
    null,
//  for an occlusion at [0,6]:
    null,
//  for an occlusion at [0,7]:
    null,
//  for an occlusion at [0,8]:
    null,
//  for an occlusion at [0,9]:
    null,
//  for an occlusion at [0,10]:
    null,
//  for an occlusion at [0,11]:
    null,
//  for an occlusion at [0,12]:
    null,
//  for an occlusion at [0,13]:
    null,
//  for an occlusion at [0,14]:
    null,
//  for an occlusion at [0,15]:
    null,
//  for an occlusion at [0,16]:
    null,
//  for an occlusion at [0,17]:
    null,
//  for an occlusion at [0,18]:
    null,
//  for an occlusion at [0,19]:
    null,
//  for an occlusion at [0,20]:
    null,
//  for an occlusion at [0,21]:
    null,
//  for an occlusion at [0,22]:
    null,
//  for an occlusion at [0,23]:
    null,
//  for an occlusion at [0,24]:
    null,
//  for an occlusion at [0,25]:
    null,
//  for an occlusion at [0,26]:
    null,
//  for an occlusion at [0,27]:
    null,
//  for an occlusion at [0,28]:
    null,
//  for an occlusion at [0,29]:
    null,
//  for an occlusion at [0,30]:
    null,
  },
  {
//  for an occlusion at [1,0]:
    null,
//  for an occlusion at [1,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
	},
//  for an occlusion at [1,2]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
	},
//  for an occlusion at [1,3]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
	},
//  for an occlusion at [1,4]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
	},
//  for an occlusion at [1,5]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2147483648u;
	},
//  for an occlusion at [1,6]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2147483648u;
	},
//  for an occlusion at [1,7]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2147483648u;
	},
//  for an occlusion at [1,8]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2147483648u;
		shadow[8,0] |= 2147483648u;
	},
//  for an occlusion at [1,9]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 2147483648u;
	},
//  for an occlusion at [1,10]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 2147483648u;
	},
//  for an occlusion at [1,11]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 2147483648u;
	},
//  for an occlusion at [1,12]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 2147483648u;
	},
//  for an occlusion at [1,13]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 2147483648u;
	},
//  for an occlusion at [1,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 2147483648u;
	},
//  for an occlusion at [1,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 2147483648u;
	},
//  for an occlusion at [1,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 2147483648u;
	},
//  for an occlusion at [1,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 2147483648u;
	},
//  for an occlusion at [1,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 2147483648u;
	},
//  for an occlusion at [1,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 2147483648u;
	},
//  for an occlusion at [1,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 2147483648u;
	},
//  for an occlusion at [1,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 2147483648u;
	},
//  for an occlusion at [1,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 2147483648u;
		shadow[23,0] |= 2147483648u;
	},
//  for an occlusion at [1,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 2147483648u;
	},
//  for an occlusion at [1,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 2147483648u;
	},
//  for an occlusion at [1,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 2147483648u;
	},
//  for an occlusion at [1,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [1,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [1,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [1,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [1,30]:
    null,
  },
  {
//  for an occlusion at [2,0]:
    null,
//  for an occlusion at [2,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1073741824u;
	},
//  for an occlusion at [2,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
	},
//  for an occlusion at [2,3]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 1073741824u;
	},
//  for an occlusion at [2,4]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
		shadow[3,0] |= 1073741824u;
	},
//  for an occlusion at [2,5]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 1073741824u;
	},
//  for an occlusion at [2,6]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 3221225472u;
		shadow[6,0] |= 1073741824u;
	},
//  for an occlusion at [2,7]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 3221225472u;
		shadow[7,0] |= 1073741824u;
	},
//  for an occlusion at [2,8]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 3221225472u;
		shadow[8,0] |= 1073741824u;
	},
//  for an occlusion at [2,9]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 1073741824u;
	},
//  for an occlusion at [2,10]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3221225472u;
		shadow[10,0] |= 1073741824u;
	},
//  for an occlusion at [2,11]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 3221225472u;
		shadow[11,0] |= 1073741824u;
	},
//  for an occlusion at [2,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 3221225472u;
		shadow[12,0] |= 3221225472u;
	},
//  for an occlusion at [2,13]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 3221225472u;
	},
//  for an occlusion at [2,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 3221225472u;
	},
//  for an occlusion at [2,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 3221225472u;
	},
//  for an occlusion at [2,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 3221225472u;
	},
//  for an occlusion at [2,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 3221225472u;
	},
//  for an occlusion at [2,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 3221225472u;
		shadow[19,0] |= 3221225472u;
	},
//  for an occlusion at [2,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 1073741824u;
		shadow[20,0] |= 3221225472u;
	},
//  for an occlusion at [2,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 1073741824u;
		shadow[21,0] |= 3221225472u;
	},
//  for an occlusion at [2,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 1073741824u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [2,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 1073741824u;
		shadow[23,0] |= 3221225472u;
	},
//  for an occlusion at [2,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 1073741824u;
		shadow[24,0] |= 3221225472u;
	},
//  for an occlusion at [2,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 1073741824u;
		shadow[25,0] |= 3221225472u;
	},
//  for an occlusion at [2,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 1073741824u;
		shadow[26,0] |= 3221225472u;
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [2,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 1073741824u;
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [2,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 1073741824u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [2,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [2,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 1073741824u;
	},
//  for an occlusion at [2,30]:
    null,
  },
  {
//  for an occlusion at [3,0]:
    null,
//  for an occlusion at [3,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 536870912u;
	},
//  for an occlusion at [3,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1073741824u;
		shadow[1,0] |= 536870912u;
	},
//  for an occlusion at [3,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
		shadow[2,0] |= 536870912u;
	},
//  for an occlusion at [3,4]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 1073741824u;
		shadow[3,0] |= 536870912u;
	},
//  for an occlusion at [3,5]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
		shadow[3,0] |= 3221225472u;
		shadow[4,0] |= 1610612736u;
		shadow[5,0] |= 536870912u;
	},
//  for an occlusion at [3,6]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 1610612736u;
		shadow[6,0] |= 536870912u;
	},
//  for an occlusion at [3,7]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2147483648u;
		shadow[6,0] |= 1610612736u;
		shadow[7,0] |= 536870912u;
	},
//  for an occlusion at [3,8]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2147483648u;
		shadow[7,0] |= 1610612736u;
		shadow[8,0] |= 536870912u;
	},
//  for an occlusion at [3,9]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2147483648u;
		shadow[8,0] |= 3758096384u;
		shadow[9,0] |= 536870912u;
	},
//  for an occlusion at [3,10]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3758096384u;
		shadow[10,0] |= 536870912u;
	},
//  for an occlusion at [3,11]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 3758096384u;
		shadow[11,0] |= 536870912u;
	},
//  for an occlusion at [3,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 3758096384u;
		shadow[12,0] |= 1610612736u;
	},
//  for an occlusion at [3,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 3758096384u;
		shadow[13,0] |= 3758096384u;
	},
//  for an occlusion at [3,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 3758096384u;
	},
//  for an occlusion at [3,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 3758096384u;
	},
//  for an occlusion at [3,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 3758096384u;
	},
//  for an occlusion at [3,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 3758096384u;
		shadow[18,0] |= 3758096384u;
	},
//  for an occlusion at [3,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 1610612736u;
		shadow[19,0] |= 3758096384u;
	},
//  for an occlusion at [3,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 536870912u;
		shadow[20,0] |= 3758096384u;
	},
//  for an occlusion at [3,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 536870912u;
		shadow[21,0] |= 3758096384u;
	},
//  for an occlusion at [3,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 536870912u;
		shadow[22,0] |= 3758096384u;
		shadow[23,0] |= 2147483648u;
	},
//  for an occlusion at [3,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 536870912u;
		shadow[23,0] |= 1610612736u;
		shadow[24,0] |= 2147483648u;
	},
//  for an occlusion at [3,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 536870912u;
		shadow[24,0] |= 1610612736u;
		shadow[25,0] |= 2147483648u;
	},
//  for an occlusion at [3,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 536870912u;
		shadow[25,0] |= 1610612736u;
		shadow[26,0] |= 3221225472u;
	},
//  for an occlusion at [3,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 536870912u;
		shadow[26,0] |= 1610612736u;
		shadow[27,0] |= 3221225472u;
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [3,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 536870912u;
		shadow[28,0] |= 1073741824u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [3,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 536870912u;
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [3,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 536870912u;
		shadow[30,0] |= 1073741824u;
	},
//  for an occlusion at [3,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 536870912u;
	},
//  for an occlusion at [3,30]:
    null,
  },
  {
//  for an occlusion at [4,0]:
    null,
//  for an occlusion at [4,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 268435456u;
	},
//  for an occlusion at [4,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 536870912u;
		shadow[1,0] |= 268435456u;
	},
//  for an occlusion at [4,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1073741824u;
		shadow[1,0] |= 536870912u;
		shadow[2,0] |= 268435456u;
	},
//  for an occlusion at [4,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
		shadow[2,0] |= 536870912u;
		shadow[3,0] |= 268435456u;
	},
//  for an occlusion at [4,5]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 3221225472u;
		shadow[3,0] |= 1610612736u;
		shadow[4,0] |= 805306368u;
		shadow[5,0] |= 268435456u;
	},
//  for an occlusion at [4,6]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 3221225472u;
		shadow[4,0] |= 1610612736u;
		shadow[5,0] |= 805306368u;
		shadow[6,0] |= 268435456u;
	},
//  for an occlusion at [4,7]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2147483648u;
		shadow[5,0] |= 1610612736u;
		shadow[6,0] |= 805306368u;
		shadow[7,0] |= 268435456u;
	},
//  for an occlusion at [4,8]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2147483648u;
		shadow[6,0] |= 3758096384u;
		shadow[7,0] |= 1879048192u;
		shadow[8,0] |= 268435456u;
	},
//  for an occlusion at [4,9]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 3221225472u;
		shadow[8,0] |= 1879048192u;
		shadow[9,0] |= 268435456u;
	},
//  for an occlusion at [4,10]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 1879048192u;
		shadow[10,0] |= 268435456u;
	},
//  for an occlusion at [4,11]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3221225472u;
		shadow[10,0] |= 4026531840u;
		shadow[11,0] |= 805306368u;
	},
//  for an occlusion at [4,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 4026531840u;
		shadow[12,0] |= 805306368u;
	},
//  for an occlusion at [4,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 4026531840u;
		shadow[13,0] |= 1879048192u;
	},
//  for an occlusion at [4,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 4026531840u;
		shadow[14,0] |= 4026531840u;
	},
//  for an occlusion at [4,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 4026531840u;
	},
//  for an occlusion at [4,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 4026531840u;
		shadow[17,0] |= 4026531840u;
	},
//  for an occlusion at [4,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 1879048192u;
		shadow[18,0] |= 4026531840u;
	},
//  for an occlusion at [4,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 805306368u;
		shadow[19,0] |= 4026531840u;
	},
//  for an occlusion at [4,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 805306368u;
		shadow[20,0] |= 4026531840u;
		shadow[21,0] |= 3221225472u;
	},
//  for an occlusion at [4,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 268435456u;
		shadow[21,0] |= 1879048192u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [4,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 268435456u;
		shadow[22,0] |= 1879048192u;
		shadow[23,0] |= 3221225472u;
	},
//  for an occlusion at [4,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 268435456u;
		shadow[23,0] |= 1879048192u;
		shadow[24,0] |= 3758096384u;
		shadow[25,0] |= 2147483648u;
	},
//  for an occlusion at [4,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 268435456u;
		shadow[24,0] |= 805306368u;
		shadow[25,0] |= 1610612736u;
		shadow[26,0] |= 2147483648u;
	},
//  for an occlusion at [4,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 268435456u;
		shadow[25,0] |= 805306368u;
		shadow[26,0] |= 1610612736u;
		shadow[27,0] |= 3221225472u;
	},
//  for an occlusion at [4,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 268435456u;
		shadow[26,0] |= 805306368u;
		shadow[27,0] |= 1610612736u;
		shadow[28,0] |= 3221225472u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [4,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 268435456u;
		shadow[28,0] |= 536870912u;
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [4,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 268435456u;
		shadow[29,0] |= 536870912u;
		shadow[30,0] |= 1073741824u;
	},
//  for an occlusion at [4,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 268435456u;
		shadow[30,0] |= 536870912u;
	},
//  for an occlusion at [4,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 268435456u;
	},
//  for an occlusion at [4,30]:
    null,
  },
  {
//  for an occlusion at [5,0]:
    null,
//  for an occlusion at [5,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 134217728u;
	},
//  for an occlusion at [5,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 402653184u;
		shadow[1,0] |= 201326592u;
	},
//  for an occlusion at [5,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 805306368u;
		shadow[1,0] |= 402653184u;
		shadow[2,0] |= 201326592u;
	},
//  for an occlusion at [5,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1610612736u;
		shadow[1,0] |= 805306368u;
		shadow[2,0] |= 402653184u;
		shadow[3,0] |= 201326592u;
	},
//  for an occlusion at [5,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
		shadow[2,0] |= 536870912u;
		shadow[3,0] |= 268435456u;
		shadow[4,0] |= 134217728u;
	},
//  for an occlusion at [5,6]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 3221225472u;
		shadow[3,0] |= 1610612736u;
		shadow[4,0] |= 805306368u;
		shadow[5,0] |= 402653184u;
		shadow[6,0] |= 134217728u;
	},
//  for an occlusion at [5,7]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
		shadow[4,0] |= 1610612736u;
		shadow[5,0] |= 805306368u;
		shadow[6,0] |= 402653184u;
		shadow[7,0] |= 134217728u;
	},
//  for an occlusion at [5,8]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2147483648u;
		shadow[5,0] |= 3758096384u;
		shadow[6,0] |= 1879048192u;
		shadow[7,0] |= 402653184u;
		shadow[8,0] |= 134217728u;
	},
//  for an occlusion at [5,9]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 3221225472u;
		shadow[7,0] |= 1879048192u;
		shadow[8,0] |= 939524096u;
		shadow[9,0] |= 134217728u;
	},
//  for an occlusion at [5,10]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2147483648u;
		shadow[8,0] |= 3758096384u;
		shadow[9,0] |= 939524096u;
		shadow[10,0] |= 134217728u;
	},
//  for an occlusion at [5,11]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3758096384u;
		shadow[10,0] |= 2013265920u;
		shadow[11,0] |= 402653184u;
	},
//  for an occlusion at [5,12]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 3221225472u;
		shadow[11,0] |= 4160749568u;
		shadow[12,0] |= 939524096u;
	},
//  for an occlusion at [5,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 4160749568u;
		shadow[13,0] |= 939524096u;
	},
//  for an occlusion at [5,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 4160749568u;
		shadow[14,0] |= 4160749568u;
	},
//  for an occlusion at [5,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 4160749568u;
	},
//  for an occlusion at [5,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 4160749568u;
		shadow[17,0] |= 4160749568u;
	},
//  for an occlusion at [5,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 939524096u;
		shadow[18,0] |= 4160749568u;
	},
//  for an occlusion at [5,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 939524096u;
		shadow[19,0] |= 4160749568u;
		shadow[20,0] |= 3221225472u;
	},
//  for an occlusion at [5,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 402653184u;
		shadow[20,0] |= 2013265920u;
		shadow[21,0] |= 3758096384u;
	},
//  for an occlusion at [5,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 134217728u;
		shadow[21,0] |= 939524096u;
		shadow[22,0] |= 3758096384u;
		shadow[23,0] |= 2147483648u;
	},
//  for an occlusion at [5,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 134217728u;
		shadow[22,0] |= 939524096u;
		shadow[23,0] |= 1879048192u;
		shadow[24,0] |= 3221225472u;
	},
//  for an occlusion at [5,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 134217728u;
		shadow[23,0] |= 402653184u;
		shadow[24,0] |= 1879048192u;
		shadow[25,0] |= 3758096384u;
		shadow[26,0] |= 2147483648u;
	},
//  for an occlusion at [5,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 134217728u;
		shadow[24,0] |= 402653184u;
		shadow[25,0] |= 805306368u;
		shadow[26,0] |= 1610612736u;
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [5,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 134217728u;
		shadow[25,0] |= 402653184u;
		shadow[26,0] |= 805306368u;
		shadow[27,0] |= 1610612736u;
		shadow[28,0] |= 3221225472u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [5,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 134217728u;
		shadow[27,0] |= 268435456u;
		shadow[28,0] |= 536870912u;
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [5,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 201326592u;
		shadow[28,0] |= 402653184u;
		shadow[29,0] |= 805306368u;
		shadow[30,0] |= 1610612736u;
	},
//  for an occlusion at [5,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 201326592u;
		shadow[29,0] |= 402653184u;
		shadow[30,0] |= 805306368u;
	},
//  for an occlusion at [5,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 201326592u;
		shadow[30,0] |= 402653184u;
	},
//  for an occlusion at [5,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 134217728u;
	},
//  for an occlusion at [5,30]:
    null,
  },
  {
//  for an occlusion at [6,0]:
    null,
//  for an occlusion at [6,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 67108864u;
	},
//  for an occlusion at [6,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 67108864u;
		shadow[1,0] |= 100663296u;
	},
//  for an occlusion at [6,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 134217728u;
		shadow[1,0] |= 201326592u;
		shadow[2,0] |= 100663296u;
	},
//  for an occlusion at [6,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 268435456u;
		shadow[1,0] |= 402653184u;
		shadow[2,0] |= 201326592u;
		shadow[3,0] |= 100663296u;
	},
//  for an occlusion at [6,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1610612736u;
		shadow[1,0] |= 805306368u;
		shadow[2,0] |= 402653184u;
		shadow[3,0] |= 201326592u;
		shadow[4,0] |= 100663296u;
	},
//  for an occlusion at [6,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
		shadow[2,0] |= 536870912u;
		shadow[3,0] |= 268435456u;
		shadow[4,0] |= 134217728u;
		shadow[5,0] |= 67108864u;
	},
//  for an occlusion at [6,7]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 3221225472u;
		shadow[3,0] |= 1610612736u;
		shadow[4,0] |= 805306368u;
		shadow[5,0] |= 402653184u;
		shadow[6,0] |= 201326592u;
		shadow[7,0] |= 67108864u;
	},
//  for an occlusion at [6,8]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 1879048192u;
		shadow[6,0] |= 402653184u;
		shadow[7,0] |= 201326592u;
		shadow[8,0] |= 67108864u;
	},
//  for an occlusion at [6,9]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 3221225472u;
		shadow[6,0] |= 1879048192u;
		shadow[7,0] |= 939524096u;
		shadow[8,0] |= 469762048u;
		shadow[9,0] |= 67108864u;
	},
//  for an occlusion at [6,10]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2147483648u;
		shadow[7,0] |= 3758096384u;
		shadow[8,0] |= 2013265920u;
		shadow[9,0] |= 469762048u;
		shadow[10,0] |= 67108864u;
	},
//  for an occlusion at [6,11]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 4026531840u;
		shadow[10,0] |= 1006632960u;
		shadow[11,0] |= 201326592u;
	},
//  for an occlusion at [6,12]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 4026531840u;
		shadow[11,0] |= 2080374784u;
		shadow[12,0] |= 469762048u;
	},
//  for an occlusion at [6,13]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 3221225472u;
		shadow[12,0] |= 4227858432u;
		shadow[13,0] |= 469762048u;
	},
//  for an occlusion at [6,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 4227858432u;
		shadow[14,0] |= 4227858432u;
	},
//  for an occlusion at [6,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 4227858432u;
	},
//  for an occlusion at [6,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 4227858432u;
		shadow[17,0] |= 4227858432u;
	},
//  for an occlusion at [6,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 469762048u;
		shadow[18,0] |= 4227858432u;
		shadow[19,0] |= 3221225472u;
	},
//  for an occlusion at [6,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 469762048u;
		shadow[19,0] |= 2080374784u;
		shadow[20,0] |= 4026531840u;
	},
//  for an occlusion at [6,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 201326592u;
		shadow[20,0] |= 1006632960u;
		shadow[21,0] |= 4026531840u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [6,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 67108864u;
		shadow[21,0] |= 469762048u;
		shadow[22,0] |= 2013265920u;
		shadow[23,0] |= 3758096384u;
		shadow[24,0] |= 2147483648u;
	},
//  for an occlusion at [6,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 67108864u;
		shadow[22,0] |= 469762048u;
		shadow[23,0] |= 939524096u;
		shadow[24,0] |= 1879048192u;
		shadow[25,0] |= 3221225472u;
	},
//  for an occlusion at [6,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 67108864u;
		shadow[23,0] |= 201326592u;
		shadow[24,0] |= 402653184u;
		shadow[25,0] |= 1879048192u;
		shadow[26,0] |= 3221225472u;
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [6,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 67108864u;
		shadow[24,0] |= 201326592u;
		shadow[25,0] |= 402653184u;
		shadow[26,0] |= 805306368u;
		shadow[27,0] |= 1610612736u;
		shadow[28,0] |= 3221225472u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [6,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 67108864u;
		shadow[26,0] |= 134217728u;
		shadow[27,0] |= 268435456u;
		shadow[28,0] |= 536870912u;
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [6,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 100663296u;
		shadow[27,0] |= 201326592u;
		shadow[28,0] |= 402653184u;
		shadow[29,0] |= 805306368u;
		shadow[30,0] |= 1610612736u;
	},
//  for an occlusion at [6,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 100663296u;
		shadow[28,0] |= 201326592u;
		shadow[29,0] |= 402653184u;
		shadow[30,0] |= 268435456u;
	},
//  for an occlusion at [6,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 100663296u;
		shadow[29,0] |= 201326592u;
		shadow[30,0] |= 134217728u;
	},
//  for an occlusion at [6,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 100663296u;
		shadow[30,0] |= 67108864u;
	},
//  for an occlusion at [6,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 67108864u;
	},
//  for an occlusion at [6,30]:
    null,
  },
  {
//  for an occlusion at [7,0]:
    null,
//  for an occlusion at [7,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 33554432u;
	},
//  for an occlusion at [7,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 33554432u;
		shadow[1,0] |= 50331648u;
	},
//  for an occlusion at [7,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 67108864u;
		shadow[1,0] |= 33554432u;
		shadow[2,0] |= 50331648u;
	},
//  for an occlusion at [7,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 134217728u;
		shadow[1,0] |= 67108864u;
		shadow[2,0] |= 100663296u;
		shadow[3,0] |= 50331648u;
	},
//  for an occlusion at [7,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 268435456u;
		shadow[1,0] |= 134217728u;
		shadow[2,0] |= 201326592u;
		shadow[3,0] |= 100663296u;
		shadow[4,0] |= 50331648u;
	},
//  for an occlusion at [7,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1610612736u;
		shadow[1,0] |= 805306368u;
		shadow[2,0] |= 402653184u;
		shadow[3,0] |= 201326592u;
		shadow[4,0] |= 100663296u;
		shadow[5,0] |= 50331648u;
	},
//  for an occlusion at [7,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2147483648u;
		shadow[1,0] |= 1073741824u;
		shadow[2,0] |= 536870912u;
		shadow[3,0] |= 268435456u;
		shadow[4,0] |= 134217728u;
		shadow[5,0] |= 67108864u;
		shadow[6,0] |= 33554432u;
	},
//  for an occlusion at [7,8]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2147483648u;
		shadow[2,0] |= 3221225472u;
		shadow[3,0] |= 1610612736u;
		shadow[4,0] |= 805306368u;
		shadow[5,0] |= 402653184u;
		shadow[6,0] |= 201326592u;
		shadow[7,0] |= 100663296u;
		shadow[8,0] |= 33554432u;
	},
//  for an occlusion at [7,9]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 1879048192u;
		shadow[6,0] |= 939524096u;
		shadow[7,0] |= 469762048u;
		shadow[8,0] |= 100663296u;
		shadow[9,0] |= 33554432u;
	},
//  for an occlusion at [7,10]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2147483648u;
		shadow[6,0] |= 3758096384u;
		shadow[7,0] |= 1879048192u;
		shadow[8,0] |= 1006632960u;
		shadow[9,0] |= 234881024u;
		shadow[10,0] |= 33554432u;
	},
//  for an occlusion at [7,11]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2147483648u;
		shadow[8,0] |= 3758096384u;
		shadow[9,0] |= 939524096u;
		shadow[10,0] |= 234881024u;
		shadow[11,0] |= 33554432u;
	},
//  for an occlusion at [7,12]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3758096384u;
		shadow[10,0] |= 4160749568u;
		shadow[11,0] |= 1040187392u;
		shadow[12,0] |= 100663296u;
	},
//  for an occlusion at [7,13]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 4026531840u;
		shadow[12,0] |= 4261412864u;
		shadow[13,0] |= 503316480u;
	},
//  for an occlusion at [7,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 4261412864u;
		shadow[14,0] |= 4261412864u;
	},
//  for an occlusion at [7,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 4261412864u;
	},
//  for an occlusion at [7,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 4261412864u;
		shadow[17,0] |= 4261412864u;
	},
//  for an occlusion at [7,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 503316480u;
		shadow[18,0] |= 4261412864u;
		shadow[19,0] |= 4026531840u;
	},
//  for an occlusion at [7,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 100663296u;
		shadow[19,0] |= 1040187392u;
		shadow[20,0] |= 4160749568u;
		shadow[21,0] |= 3758096384u;
	},
//  for an occlusion at [7,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 33554432u;
		shadow[20,0] |= 234881024u;
		shadow[21,0] |= 939524096u;
		shadow[22,0] |= 3758096384u;
		shadow[23,0] |= 2147483648u;
	},
//  for an occlusion at [7,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 33554432u;
		shadow[21,0] |= 234881024u;
		shadow[22,0] |= 1006632960u;
		shadow[23,0] |= 1879048192u;
		shadow[24,0] |= 3758096384u;
		shadow[25,0] |= 2147483648u;
	},
//  for an occlusion at [7,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 33554432u;
		shadow[22,0] |= 100663296u;
		shadow[23,0] |= 469762048u;
		shadow[24,0] |= 939524096u;
		shadow[25,0] |= 1879048192u;
		shadow[26,0] |= 3221225472u;
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [7,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 33554432u;
		shadow[23,0] |= 100663296u;
		shadow[24,0] |= 201326592u;
		shadow[25,0] |= 402653184u;
		shadow[26,0] |= 805306368u;
		shadow[27,0] |= 1610612736u;
		shadow[28,0] |= 3221225472u;
		shadow[29,0] |= 2147483648u;
	},
//  for an occlusion at [7,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 33554432u;
		shadow[25,0] |= 67108864u;
		shadow[26,0] |= 134217728u;
		shadow[27,0] |= 268435456u;
		shadow[28,0] |= 536870912u;
		shadow[29,0] |= 1073741824u;
		shadow[30,0] |= 2147483648u;
	},
//  for an occlusion at [7,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 50331648u;
		shadow[26,0] |= 100663296u;
		shadow[27,0] |= 201326592u;
		shadow[28,0] |= 402653184u;
		shadow[29,0] |= 805306368u;
		shadow[30,0] |= 1610612736u;
	},
//  for an occlusion at [7,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 50331648u;
		shadow[27,0] |= 100663296u;
		shadow[28,0] |= 201326592u;
		shadow[29,0] |= 134217728u;
		shadow[30,0] |= 268435456u;
	},
//  for an occlusion at [7,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 50331648u;
		shadow[28,0] |= 100663296u;
		shadow[29,0] |= 67108864u;
		shadow[30,0] |= 134217728u;
	},
//  for an occlusion at [7,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 50331648u;
		shadow[29,0] |= 33554432u;
		shadow[30,0] |= 67108864u;
	},
//  for an occlusion at [7,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 50331648u;
		shadow[30,0] |= 33554432u;
	},
//  for an occlusion at [7,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 33554432u;
	},
//  for an occlusion at [7,30]:
    null,
  },
  {
//  for an occlusion at [8,0]:
    null,
//  for an occlusion at [8,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 25165824u;
	},
//  for an occlusion at [8,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16777216u;
		shadow[1,0] |= 25165824u;
	},
//  for an occlusion at [8,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 33554432u;
		shadow[1,0] |= 16777216u;
		shadow[2,0] |= 25165824u;
	},
//  for an occlusion at [8,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 100663296u;
		shadow[1,0] |= 50331648u;
		shadow[2,0] |= 50331648u;
		shadow[3,0] |= 25165824u;
	},
//  for an occlusion at [8,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 201326592u;
		shadow[1,0] |= 100663296u;
		shadow[2,0] |= 100663296u;
		shadow[3,0] |= 50331648u;
		shadow[4,0] |= 25165824u;
	},
//  for an occlusion at [8,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 402653184u;
		shadow[1,0] |= 201326592u;
		shadow[2,0] |= 67108864u;
		shadow[3,0] |= 100663296u;
		shadow[4,0] |= 50331648u;
		shadow[5,0] |= 25165824u;
	},
//  for an occlusion at [8,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1610612736u;
		shadow[1,0] |= 805306368u;
		shadow[2,0] |= 402653184u;
		shadow[3,0] |= 201326592u;
		shadow[4,0] |= 100663296u;
		shadow[5,0] |= 50331648u;
		shadow[6,0] |= 25165824u;
	},
//  for an occlusion at [8,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3221225472u;
		shadow[1,0] |= 3758096384u;
		shadow[2,0] |= 1879048192u;
		shadow[3,0] |= 939524096u;
		shadow[4,0] |= 469762048u;
		shadow[5,0] |= 234881024u;
		shadow[6,0] |= 117440512u;
		shadow[7,0] |= 58720256u;
		shadow[8,0] |= 16777216u;
	},
//  for an occlusion at [8,9]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 3221225472u;
		shadow[3,0] |= 3758096384u;
		shadow[4,0] |= 1879048192u;
		shadow[5,0] |= 939524096u;
		shadow[6,0] |= 201326592u;
		shadow[7,0] |= 100663296u;
		shadow[8,0] |= 50331648u;
		shadow[9,0] |= 16777216u;
	},
//  for an occlusion at [8,10]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 3758096384u;
		shadow[6,0] |= 2013265920u;
		shadow[7,0] |= 469762048u;
		shadow[8,0] |= 234881024u;
		shadow[9,0] |= 50331648u;
		shadow[10,0] |= 16777216u;
	},
//  for an occlusion at [8,11]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 3221225472u;
		shadow[7,0] |= 4026531840u;
		shadow[8,0] |= 2013265920u;
		shadow[9,0] |= 503316480u;
		shadow[10,0] |= 117440512u;
		shadow[11,0] |= 16777216u;
	},
//  for an occlusion at [8,12]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 4026531840u;
		shadow[10,0] |= 1006632960u;
		shadow[11,0] |= 251658240u;
		shadow[12,0] |= 50331648u;
	},
//  for an occlusion at [8,13]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 3758096384u;
		shadow[11,0] |= 4227858432u;
		shadow[12,0] |= 1056964608u;
		shadow[13,0] |= 117440512u;
	},
//  for an occlusion at [8,14]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 4026531840u;
		shadow[13,0] |= 4278190080u;
		shadow[14,0] |= 1056964608u;
	},
//  for an occlusion at [8,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 4278190080u;
		shadow[15,0] |= 4278190080u;
		shadow[16,0] |= 4278190080u;
	},
//  for an occlusion at [8,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 1056964608u;
		shadow[17,0] |= 4278190080u;
		shadow[18,0] |= 4026531840u;
	},
//  for an occlusion at [8,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 117440512u;
		shadow[18,0] |= 1056964608u;
		shadow[19,0] |= 4227858432u;
		shadow[20,0] |= 3758096384u;
	},
//  for an occlusion at [8,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 50331648u;
		shadow[19,0] |= 251658240u;
		shadow[20,0] |= 1006632960u;
		shadow[21,0] |= 4026531840u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [8,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 16777216u;
		shadow[20,0] |= 117440512u;
		shadow[21,0] |= 503316480u;
		shadow[22,0] |= 2013265920u;
		shadow[23,0] |= 4026531840u;
		shadow[24,0] |= 3221225472u;
	},
//  for an occlusion at [8,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 16777216u;
		shadow[21,0] |= 50331648u;
		shadow[22,0] |= 234881024u;
		shadow[23,0] |= 469762048u;
		shadow[24,0] |= 2013265920u;
		shadow[25,0] |= 3758096384u;
		shadow[26,0] |= 3221225472u;
	},
//  for an occlusion at [8,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 16777216u;
		shadow[22,0] |= 50331648u;
		shadow[23,0] |= 100663296u;
		shadow[24,0] |= 201326592u;
		shadow[25,0] |= 939524096u;
		shadow[26,0] |= 1879048192u;
		shadow[27,0] |= 3758096384u;
		shadow[28,0] |= 3221225472u;
	},
//  for an occlusion at [8,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 16777216u;
		shadow[23,0] |= 58720256u;
		shadow[24,0] |= 117440512u;
		shadow[25,0] |= 234881024u;
		shadow[26,0] |= 469762048u;
		shadow[27,0] |= 939524096u;
		shadow[28,0] |= 1879048192u;
		shadow[29,0] |= 3758096384u;
		shadow[30,0] |= 3221225472u;
	},
//  for an occlusion at [8,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 25165824u;
		shadow[25,0] |= 50331648u;
		shadow[26,0] |= 100663296u;
		shadow[27,0] |= 201326592u;
		shadow[28,0] |= 402653184u;
		shadow[29,0] |= 805306368u;
		shadow[30,0] |= 1610612736u;
	},
//  for an occlusion at [8,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 25165824u;
		shadow[26,0] |= 50331648u;
		shadow[27,0] |= 100663296u;
		shadow[28,0] |= 67108864u;
		shadow[29,0] |= 201326592u;
		shadow[30,0] |= 402653184u;
	},
//  for an occlusion at [8,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 25165824u;
		shadow[27,0] |= 50331648u;
		shadow[28,0] |= 100663296u;
		shadow[29,0] |= 100663296u;
		shadow[30,0] |= 201326592u;
	},
//  for an occlusion at [8,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 25165824u;
		shadow[28,0] |= 50331648u;
		shadow[29,0] |= 50331648u;
		shadow[30,0] |= 100663296u;
	},
//  for an occlusion at [8,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 25165824u;
		shadow[29,0] |= 16777216u;
		shadow[30,0] |= 33554432u;
	},
//  for an occlusion at [8,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 25165824u;
		shadow[30,0] |= 16777216u;
	},
//  for an occlusion at [8,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 25165824u;
	},
//  for an occlusion at [8,30]:
    null,
  },
  {
//  for an occlusion at [9,0]:
    null,
//  for an occlusion at [9,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4194304u;
	},
//  for an occlusion at [9,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8388608u;
		shadow[1,0] |= 12582912u;
	},
//  for an occlusion at [9,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 25165824u;
		shadow[1,0] |= 8388608u;
		shadow[2,0] |= 12582912u;
	},
//  for an occlusion at [9,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16777216u;
		shadow[1,0] |= 25165824u;
		shadow[2,0] |= 8388608u;
		shadow[3,0] |= 12582912u;
	},
//  for an occlusion at [9,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 33554432u;
		shadow[1,0] |= 50331648u;
		shadow[2,0] |= 25165824u;
		shadow[3,0] |= 25165824u;
		shadow[4,0] |= 12582912u;
	},
//  for an occlusion at [9,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 67108864u;
		shadow[1,0] |= 100663296u;
		shadow[2,0] |= 50331648u;
		shadow[3,0] |= 58720256u;
		shadow[4,0] |= 25165824u;
		shadow[5,0] |= 12582912u;
	},
//  for an occlusion at [9,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 402653184u;
		shadow[1,0] |= 201326592u;
		shadow[2,0] |= 100663296u;
		shadow[3,0] |= 117440512u;
		shadow[4,0] |= 50331648u;
		shadow[5,0] |= 25165824u;
		shadow[6,0] |= 12582912u;
	},
//  for an occlusion at [9,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 805306368u;
		shadow[1,0] |= 939524096u;
		shadow[2,0] |= 469762048u;
		shadow[3,0] |= 201326592u;
		shadow[4,0] |= 100663296u;
		shadow[5,0] |= 50331648u;
		shadow[6,0] |= 25165824u;
		shadow[7,0] |= 12582912u;
	},
//  for an occlusion at [9,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3221225472u;
		shadow[1,0] |= 3758096384u;
		shadow[2,0] |= 1879048192u;
		shadow[3,0] |= 939524096u;
		shadow[4,0] |= 469762048u;
		shadow[5,0] |= 234881024u;
		shadow[6,0] |= 117440512u;
		shadow[7,0] |= 58720256u;
		shadow[8,0] |= 29360128u;
		shadow[9,0] |= 8388608u;
	},
//  for an occlusion at [9,10]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
		shadow[3,0] |= 3221225472u;
		shadow[4,0] |= 1879048192u;
		shadow[5,0] |= 939524096u;
		shadow[6,0] |= 469762048u;
		shadow[7,0] |= 234881024u;
		shadow[8,0] |= 50331648u;
		shadow[9,0] |= 25165824u;
		shadow[10,0] |= 8388608u;
	},
//  for an occlusion at [9,11]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2147483648u;
		shadow[5,0] |= 3758096384u;
		shadow[6,0] |= 4026531840u;
		shadow[7,0] |= 2013265920u;
		shadow[8,0] |= 503316480u;
		shadow[9,0] |= 117440512u;
		shadow[10,0] |= 58720256u;
		shadow[11,0] |= 8388608u;
	},
//  for an occlusion at [9,12]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 3221225472u;
		shadow[8,0] |= 4026531840u;
		shadow[9,0] |= 2080374784u;
		shadow[10,0] |= 520093696u;
		shadow[11,0] |= 125829120u;
		shadow[12,0] |= 25165824u;
	},
//  for an occlusion at [9,13]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 3221225472u;
		shadow[10,0] |= 4160749568u;
		shadow[11,0] |= 4261412864u;
		shadow[12,0] |= 1065353216u;
		shadow[13,0] |= 58720256u;
	},
//  for an occlusion at [9,14]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 4160749568u;
		shadow[13,0] |= 4286578688u;
		shadow[14,0] |= 260046848u;
	},
//  for an occlusion at [9,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 4286578688u;
		shadow[15,0] |= 4286578688u;
		shadow[16,0] |= 4286578688u;
	},
//  for an occlusion at [9,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 260046848u;
		shadow[17,0] |= 4286578688u;
		shadow[18,0] |= 4160749568u;
	},
//  for an occlusion at [9,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 58720256u;
		shadow[18,0] |= 1065353216u;
		shadow[19,0] |= 4261412864u;
		shadow[20,0] |= 4160749568u;
		shadow[21,0] |= 3221225472u;
	},
//  for an occlusion at [9,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 25165824u;
		shadow[19,0] |= 125829120u;
		shadow[20,0] |= 520093696u;
		shadow[21,0] |= 2080374784u;
		shadow[22,0] |= 4026531840u;
		shadow[23,0] |= 3221225472u;
	},
//  for an occlusion at [9,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 8388608u;
		shadow[20,0] |= 58720256u;
		shadow[21,0] |= 117440512u;
		shadow[22,0] |= 503316480u;
		shadow[23,0] |= 2013265920u;
		shadow[24,0] |= 4026531840u;
		shadow[25,0] |= 3758096384u;
		shadow[26,0] |= 2147483648u;
	},
//  for an occlusion at [9,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 8388608u;
		shadow[21,0] |= 25165824u;
		shadow[22,0] |= 50331648u;
		shadow[23,0] |= 234881024u;
		shadow[24,0] |= 469762048u;
		shadow[25,0] |= 939524096u;
		shadow[26,0] |= 1879048192u;
		shadow[27,0] |= 3221225472u;
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [9,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 8388608u;
		shadow[22,0] |= 29360128u;
		shadow[23,0] |= 58720256u;
		shadow[24,0] |= 117440512u;
		shadow[25,0] |= 234881024u;
		shadow[26,0] |= 469762048u;
		shadow[27,0] |= 939524096u;
		shadow[28,0] |= 1879048192u;
		shadow[29,0] |= 3758096384u;
		shadow[30,0] |= 3221225472u;
	},
//  for an occlusion at [9,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 12582912u;
		shadow[24,0] |= 25165824u;
		shadow[25,0] |= 50331648u;
		shadow[26,0] |= 100663296u;
		shadow[27,0] |= 201326592u;
		shadow[28,0] |= 469762048u;
		shadow[29,0] |= 939524096u;
		shadow[30,0] |= 805306368u;
	},
//  for an occlusion at [9,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 12582912u;
		shadow[25,0] |= 25165824u;
		shadow[26,0] |= 50331648u;
		shadow[27,0] |= 117440512u;
		shadow[28,0] |= 100663296u;
		shadow[29,0] |= 201326592u;
		shadow[30,0] |= 402653184u;
	},
//  for an occlusion at [9,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 12582912u;
		shadow[26,0] |= 25165824u;
		shadow[27,0] |= 58720256u;
		shadow[28,0] |= 50331648u;
		shadow[29,0] |= 100663296u;
		shadow[30,0] |= 67108864u;
	},
//  for an occlusion at [9,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 12582912u;
		shadow[27,0] |= 25165824u;
		shadow[28,0] |= 25165824u;
		shadow[29,0] |= 50331648u;
		shadow[30,0] |= 33554432u;
	},
//  for an occlusion at [9,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 12582912u;
		shadow[28,0] |= 8388608u;
		shadow[29,0] |= 25165824u;
		shadow[30,0] |= 16777216u;
	},
//  for an occlusion at [9,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 12582912u;
		shadow[29,0] |= 8388608u;
		shadow[30,0] |= 25165824u;
	},
//  for an occlusion at [9,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 12582912u;
		shadow[30,0] |= 8388608u;
	},
//  for an occlusion at [9,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 4194304u;
	},
//  for an occlusion at [9,30]:
    null,
  },
  {
//  for an occlusion at [10,0]:
    null,
//  for an occlusion at [10,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2097152u;
	},
//  for an occlusion at [10,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4194304u;
		shadow[1,0] |= 6291456u;
	},
//  for an occlusion at [10,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4194304u;
		shadow[1,0] |= 4194304u;
		shadow[2,0] |= 6291456u;
	},
//  for an occlusion at [10,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8388608u;
		shadow[1,0] |= 12582912u;
		shadow[2,0] |= 4194304u;
		shadow[3,0] |= 6291456u;
	},
//  for an occlusion at [10,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 25165824u;
		shadow[1,0] |= 8388608u;
		shadow[2,0] |= 12582912u;
		shadow[3,0] |= 4194304u;
		shadow[4,0] |= 6291456u;
	},
//  for an occlusion at [10,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 50331648u;
		shadow[1,0] |= 25165824u;
		shadow[2,0] |= 25165824u;
		shadow[3,0] |= 12582912u;
		shadow[4,0] |= 12582912u;
		shadow[5,0] |= 6291456u;
	},
//  for an occlusion at [10,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 100663296u;
		shadow[1,0] |= 50331648u;
		shadow[2,0] |= 58720256u;
		shadow[3,0] |= 25165824u;
		shadow[4,0] |= 12582912u;
		shadow[5,0] |= 12582912u;
		shadow[6,0] |= 6291456u;
	},
//  for an occlusion at [10,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 201326592u;
		shadow[1,0] |= 234881024u;
		shadow[2,0] |= 100663296u;
		shadow[3,0] |= 50331648u;
		shadow[4,0] |= 58720256u;
		shadow[5,0] |= 25165824u;
		shadow[6,0] |= 12582912u;
		shadow[7,0] |= 6291456u;
	},
//  for an occlusion at [10,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 805306368u;
		shadow[1,0] |= 402653184u;
		shadow[2,0] |= 201326592u;
		shadow[3,0] |= 234881024u;
		shadow[4,0] |= 117440512u;
		shadow[5,0] |= 50331648u;
		shadow[6,0] |= 25165824u;
		shadow[7,0] |= 12582912u;
		shadow[8,0] |= 6291456u;
	},
//  for an occlusion at [10,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3221225472u;
		shadow[1,0] |= 3758096384u;
		shadow[2,0] |= 1879048192u;
		shadow[3,0] |= 939524096u;
		shadow[4,0] |= 469762048u;
		shadow[5,0] |= 234881024u;
		shadow[6,0] |= 117440512u;
		shadow[7,0] |= 58720256u;
		shadow[8,0] |= 29360128u;
		shadow[9,0] |= 14680064u;
		shadow[10,0] |= 4194304u;
	},
//  for an occlusion at [10,11]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
		shadow[3,0] |= 3221225472u;
		shadow[4,0] |= 3758096384u;
		shadow[5,0] |= 2013265920u;
		shadow[6,0] |= 1006632960u;
		shadow[7,0] |= 234881024u;
		shadow[8,0] |= 117440512u;
		shadow[9,0] |= 58720256u;
		shadow[10,0] |= 12582912u;
		shadow[11,0] |= 4194304u;
	},
//  for an occlusion at [10,12]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2147483648u;
		shadow[6,0] |= 3758096384u;
		shadow[7,0] |= 4160749568u;
		shadow[8,0] |= 2080374784u;
		shadow[9,0] |= 520093696u;
		shadow[10,0] |= 125829120u;
		shadow[11,0] |= 29360128u;
		shadow[12,0] |= 4194304u;
	},
//  for an occlusion at [10,13]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 4026531840u;
		shadow[10,0] |= 4227858432u;
		shadow[11,0] |= 1056964608u;
		shadow[12,0] |= 264241152u;
		shadow[13,0] |= 29360128u;
	},
//  for an occlusion at [10,14]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 3758096384u;
		shadow[12,0] |= 4261412864u;
		shadow[13,0] |= 4290772992u;
		shadow[14,0] |= 264241152u;
	},
//  for an occlusion at [10,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 4290772992u;
		shadow[15,0] |= 4290772992u;
		shadow[16,0] |= 4290772992u;
	},
//  for an occlusion at [10,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 264241152u;
		shadow[17,0] |= 4290772992u;
		shadow[18,0] |= 4261412864u;
		shadow[19,0] |= 3758096384u;
	},
//  for an occlusion at [10,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 29360128u;
		shadow[18,0] |= 264241152u;
		shadow[19,0] |= 1056964608u;
		shadow[20,0] |= 4227858432u;
		shadow[21,0] |= 4026531840u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [10,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 4194304u;
		shadow[19,0] |= 29360128u;
		shadow[20,0] |= 125829120u;
		shadow[21,0] |= 520093696u;
		shadow[22,0] |= 2080374784u;
		shadow[23,0] |= 4160749568u;
		shadow[24,0] |= 3758096384u;
		shadow[25,0] |= 2147483648u;
	},
//  for an occlusion at [10,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 4194304u;
		shadow[20,0] |= 12582912u;
		shadow[21,0] |= 58720256u;
		shadow[22,0] |= 117440512u;
		shadow[23,0] |= 234881024u;
		shadow[24,0] |= 1006632960u;
		shadow[25,0] |= 2013265920u;
		shadow[26,0] |= 3758096384u;
		shadow[27,0] |= 3221225472u;
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [10,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 4194304u;
		shadow[21,0] |= 14680064u;
		shadow[22,0] |= 29360128u;
		shadow[23,0] |= 58720256u;
		shadow[24,0] |= 117440512u;
		shadow[25,0] |= 234881024u;
		shadow[26,0] |= 469762048u;
		shadow[27,0] |= 939524096u;
		shadow[28,0] |= 1879048192u;
		shadow[29,0] |= 3758096384u;
		shadow[30,0] |= 3221225472u;
	},
//  for an occlusion at [10,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 6291456u;
		shadow[23,0] |= 12582912u;
		shadow[24,0] |= 25165824u;
		shadow[25,0] |= 50331648u;
		shadow[26,0] |= 117440512u;
		shadow[27,0] |= 234881024u;
		shadow[28,0] |= 201326592u;
		shadow[29,0] |= 402653184u;
		shadow[30,0] |= 805306368u;
	},
//  for an occlusion at [10,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 6291456u;
		shadow[24,0] |= 12582912u;
		shadow[25,0] |= 25165824u;
		shadow[26,0] |= 58720256u;
		shadow[27,0] |= 50331648u;
		shadow[28,0] |= 100663296u;
		shadow[29,0] |= 234881024u;
		shadow[30,0] |= 201326592u;
	},
//  for an occlusion at [10,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 6291456u;
		shadow[25,0] |= 12582912u;
		shadow[26,0] |= 12582912u;
		shadow[27,0] |= 25165824u;
		shadow[28,0] |= 58720256u;
		shadow[29,0] |= 50331648u;
		shadow[30,0] |= 100663296u;
	},
//  for an occlusion at [10,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 6291456u;
		shadow[26,0] |= 12582912u;
		shadow[27,0] |= 12582912u;
		shadow[28,0] |= 25165824u;
		shadow[29,0] |= 25165824u;
		shadow[30,0] |= 50331648u;
	},
//  for an occlusion at [10,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 6291456u;
		shadow[27,0] |= 4194304u;
		shadow[28,0] |= 12582912u;
		shadow[29,0] |= 8388608u;
		shadow[30,0] |= 25165824u;
	},
//  for an occlusion at [10,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 6291456u;
		shadow[28,0] |= 4194304u;
		shadow[29,0] |= 12582912u;
		shadow[30,0] |= 8388608u;
	},
//  for an occlusion at [10,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 6291456u;
		shadow[29,0] |= 4194304u;
		shadow[30,0] |= 4194304u;
	},
//  for an occlusion at [10,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 6291456u;
		shadow[30,0] |= 4194304u;
	},
//  for an occlusion at [10,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 2097152u;
	},
//  for an occlusion at [10,30]:
    null,
  },
  {
//  for an occlusion at [11,0]:
    null,
//  for an occlusion at [11,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1048576u;
	},
//  for an occlusion at [11,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2097152u;
		shadow[1,0] |= 3145728u;
	},
//  for an occlusion at [11,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2097152u;
		shadow[1,0] |= 2097152u;
		shadow[2,0] |= 3145728u;
	},
//  for an occlusion at [11,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6291456u;
		shadow[1,0] |= 6291456u;
		shadow[2,0] |= 3145728u;
		shadow[3,0] |= 3145728u;
	},
//  for an occlusion at [11,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4194304u;
		shadow[1,0] |= 6291456u;
		shadow[2,0] |= 6291456u;
		shadow[3,0] |= 3145728u;
		shadow[4,0] |= 3145728u;
	},
//  for an occlusion at [11,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12582912u;
		shadow[1,0] |= 12582912u;
		shadow[2,0] |= 6291456u;
		shadow[3,0] |= 6291456u;
		shadow[4,0] |= 3145728u;
		shadow[5,0] |= 3145728u;
	},
//  for an occlusion at [11,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 25165824u;
		shadow[1,0] |= 8388608u;
		shadow[2,0] |= 12582912u;
		shadow[3,0] |= 4194304u;
		shadow[4,0] |= 6291456u;
		shadow[5,0] |= 2097152u;
		shadow[6,0] |= 3145728u;
	},
//  for an occlusion at [11,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 50331648u;
		shadow[1,0] |= 58720256u;
		shadow[2,0] |= 25165824u;
		shadow[3,0] |= 29360128u;
		shadow[4,0] |= 12582912u;
		shadow[5,0] |= 6291456u;
		shadow[6,0] |= 6291456u;
		shadow[7,0] |= 3145728u;
	},
//  for an occlusion at [11,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 234881024u;
		shadow[1,0] |= 117440512u;
		shadow[2,0] |= 117440512u;
		shadow[3,0] |= 58720256u;
		shadow[4,0] |= 25165824u;
		shadow[5,0] |= 12582912u;
		shadow[6,0] |= 14680064u;
		shadow[7,0] |= 6291456u;
		shadow[8,0] |= 3145728u;
	},
//  for an occlusion at [11,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 939524096u;
		shadow[1,0] |= 469762048u;
		shadow[2,0] |= 234881024u;
		shadow[3,0] |= 100663296u;
		shadow[4,0] |= 117440512u;
		shadow[5,0] |= 58720256u;
		shadow[6,0] |= 29360128u;
		shadow[7,0] |= 12582912u;
		shadow[8,0] |= 6291456u;
		shadow[9,0] |= 3145728u;
	},
//  for an occlusion at [11,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3221225472u;
		shadow[1,0] |= 3758096384u;
		shadow[2,0] |= 1879048192u;
		shadow[3,0] |= 939524096u;
		shadow[4,0] |= 469762048u;
		shadow[5,0] |= 234881024u;
		shadow[6,0] |= 117440512u;
		shadow[7,0] |= 58720256u;
		shadow[8,0] |= 29360128u;
		shadow[9,0] |= 14680064u;
		shadow[10,0] |= 7340032u;
		shadow[11,0] |= 2097152u;
	},
//  for an occlusion at [11,12]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2147483648u;
		shadow[3,0] |= 3221225472u;
		shadow[4,0] |= 3758096384u;
		shadow[5,0] |= 4160749568u;
		shadow[6,0] |= 2080374784u;
		shadow[7,0] |= 1040187392u;
		shadow[8,0] |= 251658240u;
		shadow[9,0] |= 125829120u;
		shadow[10,0] |= 29360128u;
		shadow[11,0] |= 14680064u;
		shadow[12,0] |= 2097152u;
	},
//  for an occlusion at [11,13]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2147483648u;
		shadow[7,0] |= 3758096384u;
		shadow[8,0] |= 4160749568u;
		shadow[9,0] |= 4261412864u;
		shadow[10,0] |= 1056964608u;
		shadow[11,0] |= 264241152u;
		shadow[12,0] |= 65011712u;
		shadow[13,0] |= 6291456u;
	},
//  for an occlusion at [11,14]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 3758096384u;
		shadow[11,0] |= 4227858432u;
		shadow[12,0] |= 4286578688u;
		shadow[13,0] |= 4292870144u;
		shadow[14,0] |= 266338304u;
	},
//  for an occlusion at [11,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 4292870144u;
		shadow[15,0] |= 4292870144u;
		shadow[16,0] |= 4292870144u;
	},
//  for an occlusion at [11,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 266338304u;
		shadow[17,0] |= 4292870144u;
		shadow[18,0] |= 4286578688u;
		shadow[19,0] |= 4227858432u;
		shadow[20,0] |= 3758096384u;
	},
//  for an occlusion at [11,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 6291456u;
		shadow[18,0] |= 65011712u;
		shadow[19,0] |= 264241152u;
		shadow[20,0] |= 1056964608u;
		shadow[21,0] |= 4261412864u;
		shadow[22,0] |= 4160749568u;
		shadow[23,0] |= 3758096384u;
		shadow[24,0] |= 2147483648u;
	},
//  for an occlusion at [11,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 2097152u;
		shadow[19,0] |= 14680064u;
		shadow[20,0] |= 29360128u;
		shadow[21,0] |= 125829120u;
		shadow[22,0] |= 251658240u;
		shadow[23,0] |= 1040187392u;
		shadow[24,0] |= 2080374784u;
		shadow[25,0] |= 4160749568u;
		shadow[26,0] |= 3758096384u;
		shadow[27,0] |= 3221225472u;
		shadow[28,0] |= 2147483648u;
	},
//  for an occlusion at [11,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 2097152u;
		shadow[20,0] |= 7340032u;
		shadow[21,0] |= 14680064u;
		shadow[22,0] |= 29360128u;
		shadow[23,0] |= 58720256u;
		shadow[24,0] |= 117440512u;
		shadow[25,0] |= 234881024u;
		shadow[26,0] |= 469762048u;
		shadow[27,0] |= 939524096u;
		shadow[28,0] |= 1879048192u;
		shadow[29,0] |= 3758096384u;
		shadow[30,0] |= 3221225472u;
	},
//  for an occlusion at [11,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 3145728u;
		shadow[22,0] |= 6291456u;
		shadow[23,0] |= 12582912u;
		shadow[24,0] |= 29360128u;
		shadow[25,0] |= 58720256u;
		shadow[26,0] |= 117440512u;
		shadow[27,0] |= 100663296u;
		shadow[28,0] |= 234881024u;
		shadow[29,0] |= 469762048u;
		shadow[30,0] |= 939524096u;
	},
//  for an occlusion at [11,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 3145728u;
		shadow[23,0] |= 6291456u;
		shadow[24,0] |= 14680064u;
		shadow[25,0] |= 12582912u;
		shadow[26,0] |= 25165824u;
		shadow[27,0] |= 58720256u;
		shadow[28,0] |= 117440512u;
		shadow[29,0] |= 117440512u;
		shadow[30,0] |= 234881024u;
	},
//  for an occlusion at [11,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 3145728u;
		shadow[24,0] |= 6291456u;
		shadow[25,0] |= 6291456u;
		shadow[26,0] |= 12582912u;
		shadow[27,0] |= 29360128u;
		shadow[28,0] |= 25165824u;
		shadow[29,0] |= 58720256u;
		shadow[30,0] |= 50331648u;
	},
//  for an occlusion at [11,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 3145728u;
		shadow[25,0] |= 2097152u;
		shadow[26,0] |= 6291456u;
		shadow[27,0] |= 4194304u;
		shadow[28,0] |= 12582912u;
		shadow[29,0] |= 8388608u;
		shadow[30,0] |= 25165824u;
	},
//  for an occlusion at [11,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 3145728u;
		shadow[26,0] |= 3145728u;
		shadow[27,0] |= 6291456u;
		shadow[28,0] |= 6291456u;
		shadow[29,0] |= 12582912u;
		shadow[30,0] |= 12582912u;
	},
//  for an occlusion at [11,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 3145728u;
		shadow[27,0] |= 3145728u;
		shadow[28,0] |= 6291456u;
		shadow[29,0] |= 6291456u;
		shadow[30,0] |= 4194304u;
	},
//  for an occlusion at [11,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 3145728u;
		shadow[28,0] |= 3145728u;
		shadow[29,0] |= 6291456u;
		shadow[30,0] |= 6291456u;
	},
//  for an occlusion at [11,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 3145728u;
		shadow[29,0] |= 2097152u;
		shadow[30,0] |= 2097152u;
	},
//  for an occlusion at [11,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 3145728u;
		shadow[30,0] |= 2097152u;
	},
//  for an occlusion at [11,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 1048576u;
	},
//  for an occlusion at [11,30]:
    null,
  },
  {
//  for an occlusion at [12,0]:
    null,
//  for an occlusion at [12,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 524288u;
	},
//  for an occlusion at [12,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1572864u;
		shadow[1,0] |= 1572864u;
	},
//  for an occlusion at [12,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1048576u;
		shadow[1,0] |= 1572864u;
		shadow[2,0] |= 1572864u;
	},
//  for an occlusion at [12,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1048576u;
		shadow[1,0] |= 1048576u;
		shadow[2,0] |= 1572864u;
		shadow[3,0] |= 1572864u;
	},
//  for an occlusion at [12,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3145728u;
		shadow[1,0] |= 3145728u;
		shadow[2,0] |= 1572864u;
		shadow[3,0] |= 1572864u;
		shadow[4,0] |= 1572864u;
	},
//  for an occlusion at [12,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2097152u;
		shadow[1,0] |= 3145728u;
		shadow[2,0] |= 3145728u;
		shadow[3,0] |= 3670016u;
		shadow[4,0] |= 1572864u;
		shadow[5,0] |= 1572864u;
	},
//  for an occlusion at [12,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6291456u;
		shadow[1,0] |= 6291456u;
		shadow[2,0] |= 7340032u;
		shadow[3,0] |= 3145728u;
		shadow[4,0] |= 3145728u;
		shadow[5,0] |= 1572864u;
		shadow[6,0] |= 1572864u;
	},
//  for an occlusion at [12,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12582912u;
		shadow[1,0] |= 12582912u;
		shadow[2,0] |= 6291456u;
		shadow[3,0] |= 6291456u;
		shadow[4,0] |= 3145728u;
		shadow[5,0] |= 3145728u;
		shadow[6,0] |= 1572864u;
		shadow[7,0] |= 1572864u;
	},
//  for an occlusion at [12,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 25165824u;
		shadow[1,0] |= 29360128u;
		shadow[2,0] |= 12582912u;
		shadow[3,0] |= 14680064u;
		shadow[4,0] |= 6291456u;
		shadow[5,0] |= 7340032u;
		shadow[6,0] |= 3145728u;
		shadow[7,0] |= 3670016u;
		shadow[8,0] |= 1572864u;
	},
//  for an occlusion at [12,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 117440512u;
		shadow[1,0] |= 58720256u;
		shadow[2,0] |= 58720256u;
		shadow[3,0] |= 29360128u;
		shadow[4,0] |= 29360128u;
		shadow[5,0] |= 14680064u;
		shadow[6,0] |= 6291456u;
		shadow[7,0] |= 7340032u;
		shadow[8,0] |= 3145728u;
		shadow[9,0] |= 1572864u;
	},
//  for an occlusion at [12,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1006632960u;
		shadow[1,0] |= 503316480u;
		shadow[2,0] |= 251658240u;
		shadow[3,0] |= 117440512u;
		shadow[4,0] |= 125829120u;
		shadow[5,0] |= 62914560u;
		shadow[6,0] |= 29360128u;
		shadow[7,0] |= 14680064u;
		shadow[8,0] |= 7340032u;
		shadow[9,0] |= 3145728u;
		shadow[10,0] |= 1572864u;
	},
//  for an occlusion at [12,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3758096384u;
		shadow[1,0] |= 4026531840u;
		shadow[2,0] |= 4160749568u;
		shadow[3,0] |= 2080374784u;
		shadow[4,0] |= 1040187392u;
		shadow[5,0] |= 520093696u;
		shadow[6,0] |= 251658240u;
		shadow[7,0] |= 125829120u;
		shadow[8,0] |= 29360128u;
		shadow[9,0] |= 14680064u;
		shadow[10,0] |= 7340032u;
		shadow[11,0] |= 3670016u;
		shadow[12,0] |= 1048576u;
	},
//  for an occlusion at [12,13]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2147483648u;
		shadow[4,0] |= 3221225472u;
		shadow[5,0] |= 4026531840u;
		shadow[6,0] |= 4160749568u;
		shadow[7,0] |= 4227858432u;
		shadow[8,0] |= 2130706432u;
		shadow[9,0] |= 528482304u;
		shadow[10,0] |= 130023424u;
		shadow[11,0] |= 31457280u;
		shadow[12,0] |= 7340032u;
		shadow[13,0] |= 1048576u;
	},
//  for an occlusion at [12,14]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 4026531840u;
		shadow[10,0] |= 4227858432u;
		shadow[11,0] |= 4278190080u;
		shadow[12,0] |= 4290772992u;
		shadow[13,0] |= 535822336u;
		shadow[14,0] |= 32505856u;
	},
//  for an occlusion at [12,15]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 4227858432u;
		shadow[14,0] |= 4293918720u;
		shadow[15,0] |= 4293918720u;
		shadow[16,0] |= 4293918720u;
		shadow[17,0] |= 4227858432u;
	},
//  for an occlusion at [12,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 32505856u;
		shadow[17,0] |= 535822336u;
		shadow[18,0] |= 4290772992u;
		shadow[19,0] |= 4278190080u;
		shadow[20,0] |= 4227858432u;
		shadow[21,0] |= 4026531840u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [12,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 1048576u;
		shadow[18,0] |= 7340032u;
		shadow[19,0] |= 31457280u;
		shadow[20,0] |= 130023424u;
		shadow[21,0] |= 528482304u;
		shadow[22,0] |= 2130706432u;
		shadow[23,0] |= 4227858432u;
		shadow[24,0] |= 4160749568u;
		shadow[25,0] |= 4026531840u;
		shadow[26,0] |= 3221225472u;
		shadow[27,0] |= 2147483648u;
	},
//  for an occlusion at [12,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 1048576u;
		shadow[19,0] |= 3670016u;
		shadow[20,0] |= 7340032u;
		shadow[21,0] |= 14680064u;
		shadow[22,0] |= 29360128u;
		shadow[23,0] |= 125829120u;
		shadow[24,0] |= 251658240u;
		shadow[25,0] |= 520093696u;
		shadow[26,0] |= 1040187392u;
		shadow[27,0] |= 2080374784u;
		shadow[28,0] |= 4160749568u;
		shadow[29,0] |= 4026531840u;
		shadow[30,0] |= 3758096384u;
	},
//  for an occlusion at [12,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 1572864u;
		shadow[21,0] |= 3145728u;
		shadow[22,0] |= 7340032u;
		shadow[23,0] |= 14680064u;
		shadow[24,0] |= 29360128u;
		shadow[25,0] |= 62914560u;
		shadow[26,0] |= 125829120u;
		shadow[27,0] |= 117440512u;
		shadow[28,0] |= 251658240u;
		shadow[29,0] |= 503316480u;
		shadow[30,0] |= 1006632960u;
	},
//  for an occlusion at [12,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 1572864u;
		shadow[22,0] |= 3145728u;
		shadow[23,0] |= 7340032u;
		shadow[24,0] |= 6291456u;
		shadow[25,0] |= 14680064u;
		shadow[26,0] |= 29360128u;
		shadow[27,0] |= 29360128u;
		shadow[28,0] |= 58720256u;
		shadow[29,0] |= 58720256u;
		shadow[30,0] |= 117440512u;
	},
//  for an occlusion at [12,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 1572864u;
		shadow[23,0] |= 3670016u;
		shadow[24,0] |= 3145728u;
		shadow[25,0] |= 7340032u;
		shadow[26,0] |= 6291456u;
		shadow[27,0] |= 14680064u;
		shadow[28,0] |= 12582912u;
		shadow[29,0] |= 29360128u;
		shadow[30,0] |= 25165824u;
	},
//  for an occlusion at [12,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 1572864u;
		shadow[24,0] |= 1572864u;
		shadow[25,0] |= 3145728u;
		shadow[26,0] |= 3145728u;
		shadow[27,0] |= 6291456u;
		shadow[28,0] |= 6291456u;
		shadow[29,0] |= 12582912u;
		shadow[30,0] |= 12582912u;
	},
//  for an occlusion at [12,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 1572864u;
		shadow[25,0] |= 1572864u;
		shadow[26,0] |= 3145728u;
		shadow[27,0] |= 3145728u;
		shadow[28,0] |= 7340032u;
		shadow[29,0] |= 6291456u;
		shadow[30,0] |= 6291456u;
	},
//  for an occlusion at [12,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 1572864u;
		shadow[26,0] |= 1572864u;
		shadow[27,0] |= 3670016u;
		shadow[28,0] |= 3145728u;
		shadow[29,0] |= 3145728u;
		shadow[30,0] |= 2097152u;
	},
//  for an occlusion at [12,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 1572864u;
		shadow[27,0] |= 1572864u;
		shadow[28,0] |= 1572864u;
		shadow[29,0] |= 3145728u;
		shadow[30,0] |= 3145728u;
	},
//  for an occlusion at [12,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 1572864u;
		shadow[28,0] |= 1572864u;
		shadow[29,0] |= 1048576u;
		shadow[30,0] |= 1048576u;
	},
//  for an occlusion at [12,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 1572864u;
		shadow[29,0] |= 1572864u;
		shadow[30,0] |= 1048576u;
	},
//  for an occlusion at [12,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 1572864u;
		shadow[30,0] |= 1572864u;
	},
//  for an occlusion at [12,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 524288u;
	},
//  for an occlusion at [12,30]:
    null,
  },
  {
//  for an occlusion at [13,0]:
    null,
//  for an occlusion at [13,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 262144u;
	},
//  for an occlusion at [13,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 262144u;
		shadow[1,0] |= 262144u;
	},
//  for an occlusion at [13,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 786432u;
		shadow[1,0] |= 786432u;
		shadow[2,0] |= 786432u;
	},
//  for an occlusion at [13,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 524288u;
		shadow[1,0] |= 786432u;
		shadow[2,0] |= 786432u;
		shadow[3,0] |= 786432u;
	},
//  for an occlusion at [13,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 524288u;
		shadow[1,0] |= 524288u;
		shadow[2,0] |= 786432u;
		shadow[3,0] |= 786432u;
		shadow[4,0] |= 786432u;
	},
//  for an occlusion at [13,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1572864u;
		shadow[1,0] |= 1572864u;
		shadow[2,0] |= 524288u;
		shadow[3,0] |= 786432u;
		shadow[4,0] |= 786432u;
		shadow[5,0] |= 786432u;
	},
//  for an occlusion at [13,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1572864u;
		shadow[1,0] |= 1572864u;
		shadow[2,0] |= 1572864u;
		shadow[3,0] |= 1835008u;
		shadow[4,0] |= 786432u;
		shadow[5,0] |= 786432u;
		shadow[6,0] |= 786432u;
	},
//  for an occlusion at [13,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3145728u;
		shadow[1,0] |= 3145728u;
		shadow[2,0] |= 3670016u;
		shadow[3,0] |= 1572864u;
		shadow[4,0] |= 1572864u;
		shadow[5,0] |= 1835008u;
		shadow[6,0] |= 786432u;
		shadow[7,0] |= 786432u;
	},
//  for an occlusion at [13,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 7340032u;
		shadow[1,0] |= 7340032u;
		shadow[2,0] |= 3670016u;
		shadow[3,0] |= 3670016u;
		shadow[4,0] |= 3670016u;
		shadow[5,0] |= 1572864u;
		shadow[6,0] |= 1835008u;
		shadow[7,0] |= 786432u;
		shadow[8,0] |= 786432u;
	},
//  for an occlusion at [13,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 14680064u;
		shadow[1,0] |= 14680064u;
		shadow[2,0] |= 7340032u;
		shadow[3,0] |= 7340032u;
		shadow[4,0] |= 3670016u;
		shadow[5,0] |= 3670016u;
		shadow[6,0] |= 1572864u;
		shadow[7,0] |= 1835008u;
		shadow[8,0] |= 786432u;
		shadow[9,0] |= 786432u;
	},
//  for an occlusion at [13,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 62914560u;
		shadow[1,0] |= 29360128u;
		shadow[2,0] |= 31457280u;
		shadow[3,0] |= 14680064u;
		shadow[4,0] |= 15728640u;
		shadow[5,0] |= 7340032u;
		shadow[6,0] |= 7864320u;
		shadow[7,0] |= 3670016u;
		shadow[8,0] |= 1572864u;
		shadow[9,0] |= 1835008u;
		shadow[10,0] |= 786432u;
	},
//  for an occlusion at [13,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 520093696u;
		shadow[1,0] |= 260046848u;
		shadow[2,0] |= 125829120u;
		shadow[3,0] |= 130023424u;
		shadow[4,0] |= 62914560u;
		shadow[5,0] |= 31457280u;
		shadow[6,0] |= 14680064u;
		shadow[7,0] |= 15728640u;
		shadow[8,0] |= 7340032u;
		shadow[9,0] |= 3670016u;
		shadow[10,0] |= 1572864u;
		shadow[11,0] |= 786432u;
	},
//  for an occlusion at [13,13]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4026531840u;
		shadow[1,0] |= 4160749568u;
		shadow[2,0] |= 4227858432u;
		shadow[3,0] |= 4261412864u;
		shadow[4,0] |= 2130706432u;
		shadow[5,0] |= 1056964608u;
		shadow[6,0] |= 528482304u;
		shadow[7,0] |= 264241152u;
		shadow[8,0] |= 65011712u;
		shadow[9,0] |= 31457280u;
		shadow[10,0] |= 15728640u;
		shadow[11,0] |= 3670016u;
		shadow[12,0] |= 1835008u;
		shadow[13,0] |= 524288u;
	},
//  for an occlusion at [13,14]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2147483648u;
		shadow[5,0] |= 3758096384u;
		shadow[6,0] |= 4026531840u;
		shadow[7,0] |= 4160749568u;
		shadow[8,0] |= 4261412864u;
		shadow[9,0] |= 4278190080u;
		shadow[10,0] |= 4286578688u;
		shadow[11,0] |= 4292870144u;
		shadow[12,0] |= 1072693248u;
		shadow[13,0] |= 66584576u;
		shadow[14,0] |= 3670016u;
	},
//  for an occlusion at [13,15]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 4160749568u;
		shadow[13,0] |= 4286578688u;
		shadow[14,0] |= 4294443008u;
		shadow[15,0] |= 4294443008u;
		shadow[16,0] |= 4294443008u;
		shadow[17,0] |= 4286578688u;
		shadow[18,0] |= 4160749568u;
	},
//  for an occlusion at [13,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 3670016u;
		shadow[17,0] |= 66584576u;
		shadow[18,0] |= 1072693248u;
		shadow[19,0] |= 4292870144u;
		shadow[20,0] |= 4286578688u;
		shadow[21,0] |= 4278190080u;
		shadow[22,0] |= 4261412864u;
		shadow[23,0] |= 4160749568u;
		shadow[24,0] |= 4026531840u;
		shadow[25,0] |= 3758096384u;
		shadow[26,0] |= 2147483648u;
	},
//  for an occlusion at [13,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 524288u;
		shadow[18,0] |= 1835008u;
		shadow[19,0] |= 3670016u;
		shadow[20,0] |= 15728640u;
		shadow[21,0] |= 31457280u;
		shadow[22,0] |= 65011712u;
		shadow[23,0] |= 264241152u;
		shadow[24,0] |= 528482304u;
		shadow[25,0] |= 1056964608u;
		shadow[26,0] |= 2130706432u;
		shadow[27,0] |= 4261412864u;
		shadow[28,0] |= 4227858432u;
		shadow[29,0] |= 4160749568u;
		shadow[30,0] |= 4026531840u;
	},
//  for an occlusion at [13,18]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 786432u;
		shadow[20,0] |= 1572864u;
		shadow[21,0] |= 3670016u;
		shadow[22,0] |= 7340032u;
		shadow[23,0] |= 15728640u;
		shadow[24,0] |= 14680064u;
		shadow[25,0] |= 31457280u;
		shadow[26,0] |= 62914560u;
		shadow[27,0] |= 130023424u;
		shadow[28,0] |= 125829120u;
		shadow[29,0] |= 260046848u;
		shadow[30,0] |= 520093696u;
	},
//  for an occlusion at [13,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 786432u;
		shadow[21,0] |= 1835008u;
		shadow[22,0] |= 1572864u;
		shadow[23,0] |= 3670016u;
		shadow[24,0] |= 7864320u;
		shadow[25,0] |= 7340032u;
		shadow[26,0] |= 15728640u;
		shadow[27,0] |= 14680064u;
		shadow[28,0] |= 31457280u;
		shadow[29,0] |= 29360128u;
		shadow[30,0] |= 62914560u;
	},
//  for an occlusion at [13,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 786432u;
		shadow[22,0] |= 786432u;
		shadow[23,0] |= 1835008u;
		shadow[24,0] |= 1572864u;
		shadow[25,0] |= 3670016u;
		shadow[26,0] |= 3670016u;
		shadow[27,0] |= 7340032u;
		shadow[28,0] |= 7340032u;
		shadow[29,0] |= 14680064u;
		shadow[30,0] |= 14680064u;
	},
//  for an occlusion at [13,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 786432u;
		shadow[23,0] |= 786432u;
		shadow[24,0] |= 1835008u;
		shadow[25,0] |= 1572864u;
		shadow[26,0] |= 3670016u;
		shadow[27,0] |= 3670016u;
		shadow[28,0] |= 3670016u;
		shadow[29,0] |= 7340032u;
		shadow[30,0] |= 7340032u;
	},
//  for an occlusion at [13,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 786432u;
		shadow[24,0] |= 786432u;
		shadow[25,0] |= 1835008u;
		shadow[26,0] |= 1572864u;
		shadow[27,0] |= 1572864u;
		shadow[28,0] |= 3670016u;
		shadow[29,0] |= 3145728u;
		shadow[30,0] |= 3145728u;
	},
//  for an occlusion at [13,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 786432u;
		shadow[25,0] |= 786432u;
		shadow[26,0] |= 786432u;
		shadow[27,0] |= 1835008u;
		shadow[28,0] |= 1572864u;
		shadow[29,0] |= 1572864u;
		shadow[30,0] |= 1572864u;
	},
//  for an occlusion at [13,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 786432u;
		shadow[26,0] |= 786432u;
		shadow[27,0] |= 786432u;
		shadow[28,0] |= 524288u;
		shadow[29,0] |= 1572864u;
		shadow[30,0] |= 1572864u;
	},
//  for an occlusion at [13,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 786432u;
		shadow[27,0] |= 786432u;
		shadow[28,0] |= 786432u;
		shadow[29,0] |= 524288u;
		shadow[30,0] |= 524288u;
	},
//  for an occlusion at [13,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 786432u;
		shadow[28,0] |= 786432u;
		shadow[29,0] |= 786432u;
		shadow[30,0] |= 524288u;
	},
//  for an occlusion at [13,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 786432u;
		shadow[29,0] |= 786432u;
		shadow[30,0] |= 786432u;
	},
//  for an occlusion at [13,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 262144u;
		shadow[30,0] |= 262144u;
	},
//  for an occlusion at [13,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 262144u;
	},
//  for an occlusion at [13,30]:
    null,
  },
  {
//  for an occlusion at [14,0]:
    null,
//  for an occlusion at [14,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 131072u;
	},
//  for an occlusion at [14,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 131072u;
		shadow[1,0] |= 131072u;
	},
//  for an occlusion at [14,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 131072u;
		shadow[1,0] |= 131072u;
		shadow[2,0] |= 131072u;
	},
//  for an occlusion at [14,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 393216u;
		shadow[1,0] |= 393216u;
		shadow[2,0] |= 393216u;
		shadow[3,0] |= 393216u;
	},
//  for an occlusion at [14,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 393216u;
		shadow[1,0] |= 393216u;
		shadow[2,0] |= 393216u;
		shadow[3,0] |= 393216u;
		shadow[4,0] |= 393216u;
	},
//  for an occlusion at [14,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 393216u;
		shadow[1,0] |= 393216u;
		shadow[2,0] |= 393216u;
		shadow[3,0] |= 393216u;
		shadow[4,0] |= 393216u;
		shadow[5,0] |= 393216u;
	},
//  for an occlusion at [14,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 393216u;
		shadow[1,0] |= 393216u;
		shadow[2,0] |= 393216u;
		shadow[3,0] |= 393216u;
		shadow[4,0] |= 393216u;
		shadow[5,0] |= 393216u;
		shadow[6,0] |= 393216u;
	},
//  for an occlusion at [14,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 786432u;
		shadow[1,0] |= 786432u;
		shadow[2,0] |= 917504u;
		shadow[3,0] |= 917504u;
		shadow[4,0] |= 393216u;
		shadow[5,0] |= 393216u;
		shadow[6,0] |= 393216u;
		shadow[7,0] |= 393216u;
	},
//  for an occlusion at [14,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 786432u;
		shadow[1,0] |= 786432u;
		shadow[2,0] |= 786432u;
		shadow[3,0] |= 786432u;
		shadow[4,0] |= 917504u;
		shadow[5,0] |= 393216u;
		shadow[6,0] |= 393216u;
		shadow[7,0] |= 393216u;
		shadow[8,0] |= 393216u;
	},
//  for an occlusion at [14,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1835008u;
		shadow[1,0] |= 1835008u;
		shadow[2,0] |= 1835008u;
		shadow[3,0] |= 786432u;
		shadow[4,0] |= 917504u;
		shadow[5,0] |= 917504u;
		shadow[6,0] |= 917504u;
		shadow[7,0] |= 393216u;
		shadow[8,0] |= 393216u;
		shadow[9,0] |= 393216u;
	},
//  for an occlusion at [14,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3932160u;
		shadow[1,0] |= 3932160u;
		shadow[2,0] |= 3932160u;
		shadow[3,0] |= 1835008u;
		shadow[4,0] |= 1966080u;
		shadow[5,0] |= 1966080u;
		shadow[6,0] |= 917504u;
		shadow[7,0] |= 917504u;
		shadow[8,0] |= 917504u;
		shadow[9,0] |= 393216u;
		shadow[10,0] |= 393216u;
	},
//  for an occlusion at [14,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16252928u;
		shadow[1,0] |= 16252928u;
		shadow[2,0] |= 7864320u;
		shadow[3,0] |= 8126464u;
		shadow[4,0] |= 3932160u;
		shadow[5,0] |= 3932160u;
		shadow[6,0] |= 1835008u;
		shadow[7,0] |= 1966080u;
		shadow[8,0] |= 917504u;
		shadow[9,0] |= 917504u;
		shadow[10,0] |= 393216u;
		shadow[11,0] |= 393216u;
	},
//  for an occlusion at [14,13]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 267386880u;
		shadow[1,0] |= 133169152u;
		shadow[2,0] |= 133693440u;
		shadow[3,0] |= 66584576u;
		shadow[4,0] |= 33030144u;
		shadow[5,0] |= 16252928u;
		shadow[6,0] |= 16515072u;
		shadow[7,0] |= 8126464u;
		shadow[8,0] |= 3932160u;
		shadow[9,0] |= 1835008u;
		shadow[10,0] |= 1966080u;
		shadow[11,0] |= 917504u;
		shadow[12,0] |= 393216u;
	},
//  for an occlusion at [14,14]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4278190080u;
		shadow[1,0] |= 4286578688u;
		shadow[2,0] |= 4286578688u;
		shadow[3,0] |= 4290772992u;
		shadow[4,0] |= 4290772992u;
		shadow[5,0] |= 4292870144u;
		shadow[6,0] |= 4292870144u;
		shadow[7,0] |= 4293918720u;
		shadow[8,0] |= 2146435072u;
		shadow[9,0] |= 536346624u;
		shadow[10,0] |= 133693440u;
		shadow[11,0] |= 33292288u;
		shadow[12,0] |= 8126464u;
		shadow[13,0] |= 1966080u;
		shadow[14,0] |= 262144u;
	},
//  for an occlusion at [14,15]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 3221225472u;
		shadow[9,0] |= 4026531840u;
		shadow[10,0] |= 4227858432u;
		shadow[11,0] |= 4278190080u;
		shadow[12,0] |= 4290772992u;
		shadow[13,0] |= 4293918720u;
		shadow[14,0] |= 4294705152u;
		shadow[15,0] |= 4294705152u;
		shadow[16,0] |= 4294705152u;
		shadow[17,0] |= 4293918720u;
		shadow[18,0] |= 4290772992u;
		shadow[19,0] |= 4278190080u;
		shadow[20,0] |= 4227858432u;
		shadow[21,0] |= 4026531840u;
		shadow[22,0] |= 3221225472u;
	},
//  for an occlusion at [14,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 262144u;
		shadow[17,0] |= 1966080u;
		shadow[18,0] |= 8126464u;
		shadow[19,0] |= 33292288u;
		shadow[20,0] |= 133693440u;
		shadow[21,0] |= 536346624u;
		shadow[22,0] |= 2146435072u;
		shadow[23,0] |= 4293918720u;
		shadow[24,0] |= 4292870144u;
		shadow[25,0] |= 4292870144u;
		shadow[26,0] |= 4290772992u;
		shadow[27,0] |= 4290772992u;
		shadow[28,0] |= 4286578688u;
		shadow[29,0] |= 4286578688u;
		shadow[30,0] |= 4278190080u;
	},
//  for an occlusion at [14,17]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 393216u;
		shadow[19,0] |= 917504u;
		shadow[20,0] |= 1966080u;
		shadow[21,0] |= 1835008u;
		shadow[22,0] |= 3932160u;
		shadow[23,0] |= 8126464u;
		shadow[24,0] |= 16515072u;
		shadow[25,0] |= 16252928u;
		shadow[26,0] |= 33030144u;
		shadow[27,0] |= 66584576u;
		shadow[28,0] |= 133693440u;
		shadow[29,0] |= 133169152u;
		shadow[30,0] |= 267386880u;
	},
//  for an occlusion at [14,18]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 393216u;
		shadow[20,0] |= 393216u;
		shadow[21,0] |= 917504u;
		shadow[22,0] |= 917504u;
		shadow[23,0] |= 1966080u;
		shadow[24,0] |= 1835008u;
		shadow[25,0] |= 3932160u;
		shadow[26,0] |= 3932160u;
		shadow[27,0] |= 8126464u;
		shadow[28,0] |= 7864320u;
		shadow[29,0] |= 16252928u;
		shadow[30,0] |= 16252928u;
	},
//  for an occlusion at [14,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 393216u;
		shadow[21,0] |= 393216u;
		shadow[22,0] |= 917504u;
		shadow[23,0] |= 917504u;
		shadow[24,0] |= 917504u;
		shadow[25,0] |= 1966080u;
		shadow[26,0] |= 1966080u;
		shadow[27,0] |= 1835008u;
		shadow[28,0] |= 3932160u;
		shadow[29,0] |= 3932160u;
		shadow[30,0] |= 3932160u;
	},
//  for an occlusion at [14,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 393216u;
		shadow[22,0] |= 393216u;
		shadow[23,0] |= 393216u;
		shadow[24,0] |= 917504u;
		shadow[25,0] |= 917504u;
		shadow[26,0] |= 917504u;
		shadow[27,0] |= 786432u;
		shadow[28,0] |= 1835008u;
		shadow[29,0] |= 1835008u;
		shadow[30,0] |= 1835008u;
	},
//  for an occlusion at [14,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 393216u;
		shadow[23,0] |= 393216u;
		shadow[24,0] |= 393216u;
		shadow[25,0] |= 393216u;
		shadow[26,0] |= 917504u;
		shadow[27,0] |= 786432u;
		shadow[28,0] |= 786432u;
		shadow[29,0] |= 786432u;
		shadow[30,0] |= 786432u;
	},
//  for an occlusion at [14,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 393216u;
		shadow[24,0] |= 393216u;
		shadow[25,0] |= 393216u;
		shadow[26,0] |= 393216u;
		shadow[27,0] |= 917504u;
		shadow[28,0] |= 917504u;
		shadow[29,0] |= 786432u;
		shadow[30,0] |= 786432u;
	},
//  for an occlusion at [14,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 393216u;
		shadow[25,0] |= 393216u;
		shadow[26,0] |= 393216u;
		shadow[27,0] |= 393216u;
		shadow[28,0] |= 393216u;
		shadow[29,0] |= 393216u;
		shadow[30,0] |= 393216u;
	},
//  for an occlusion at [14,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 393216u;
		shadow[26,0] |= 393216u;
		shadow[27,0] |= 393216u;
		shadow[28,0] |= 393216u;
		shadow[29,0] |= 393216u;
		shadow[30,0] |= 393216u;
	},
//  for an occlusion at [14,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 393216u;
		shadow[27,0] |= 393216u;
		shadow[28,0] |= 393216u;
		shadow[29,0] |= 393216u;
		shadow[30,0] |= 393216u;
	},
//  for an occlusion at [14,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 393216u;
		shadow[28,0] |= 393216u;
		shadow[29,0] |= 393216u;
		shadow[30,0] |= 393216u;
	},
//  for an occlusion at [14,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 131072u;
		shadow[29,0] |= 131072u;
		shadow[30,0] |= 131072u;
	},
//  for an occlusion at [14,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 131072u;
		shadow[30,0] |= 131072u;
	},
//  for an occlusion at [14,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 131072u;
	},
//  for an occlusion at [14,30]:
    null,
  },
  {
//  for an occlusion at [15,0]:
    null,
//  for an occlusion at [15,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
	},
//  for an occlusion at [15,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
	},
//  for an occlusion at [15,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
		shadow[2,0] |= 65536u;
	},
//  for an occlusion at [15,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
		shadow[2,0] |= 65536u;
		shadow[3,0] |= 65536u;
	},
//  for an occlusion at [15,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
		shadow[2,0] |= 65536u;
		shadow[3,0] |= 65536u;
		shadow[4,0] |= 65536u;
	},
//  for an occlusion at [15,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
		shadow[2,0] |= 65536u;
		shadow[3,0] |= 65536u;
		shadow[4,0] |= 65536u;
		shadow[5,0] |= 65536u;
	},
//  for an occlusion at [15,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 65536u;
		shadow[1,0] |= 65536u;
		shadow[2,0] |= 65536u;
		shadow[3,0] |= 65536u;
		shadow[4,0] |= 65536u;
		shadow[5,0] |= 65536u;
		shadow[6,0] |= 65536u;
	},
//  for an occlusion at [15,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 229376u;
		shadow[1,0] |= 229376u;
		shadow[2,0] |= 229376u;
		shadow[3,0] |= 229376u;
		shadow[4,0] |= 229376u;
		shadow[5,0] |= 229376u;
		shadow[6,0] |= 229376u;
		shadow[7,0] |= 229376u;
	},
//  for an occlusion at [15,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 229376u;
		shadow[1,0] |= 229376u;
		shadow[2,0] |= 229376u;
		shadow[3,0] |= 229376u;
		shadow[4,0] |= 229376u;
		shadow[5,0] |= 229376u;
		shadow[6,0] |= 229376u;
		shadow[7,0] |= 229376u;
		shadow[8,0] |= 229376u;
	},
//  for an occlusion at [15,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 229376u;
		shadow[1,0] |= 229376u;
		shadow[2,0] |= 229376u;
		shadow[3,0] |= 229376u;
		shadow[4,0] |= 229376u;
		shadow[5,0] |= 229376u;
		shadow[6,0] |= 229376u;
		shadow[7,0] |= 229376u;
		shadow[8,0] |= 229376u;
		shadow[9,0] |= 229376u;
	},
//  for an occlusion at [15,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 229376u;
		shadow[1,0] |= 229376u;
		shadow[2,0] |= 229376u;
		shadow[3,0] |= 229376u;
		shadow[4,0] |= 229376u;
		shadow[5,0] |= 229376u;
		shadow[6,0] |= 229376u;
		shadow[7,0] |= 229376u;
		shadow[8,0] |= 229376u;
		shadow[9,0] |= 229376u;
		shadow[10,0] |= 229376u;
	},
//  for an occlusion at [15,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 507904u;
		shadow[1,0] |= 507904u;
		shadow[2,0] |= 507904u;
		shadow[3,0] |= 507904u;
		shadow[4,0] |= 507904u;
		shadow[5,0] |= 507904u;
		shadow[6,0] |= 229376u;
		shadow[7,0] |= 229376u;
		shadow[8,0] |= 229376u;
		shadow[9,0] |= 229376u;
		shadow[10,0] |= 229376u;
		shadow[11,0] |= 229376u;
	},
//  for an occlusion at [15,13]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1040384u;
		shadow[1,0] |= 1040384u;
		shadow[2,0] |= 1040384u;
		shadow[3,0] |= 1040384u;
		shadow[4,0] |= 1040384u;
		shadow[5,0] |= 507904u;
		shadow[6,0] |= 507904u;
		shadow[7,0] |= 507904u;
		shadow[8,0] |= 507904u;
		shadow[9,0] |= 229376u;
		shadow[10,0] |= 229376u;
		shadow[11,0] |= 229376u;
		shadow[12,0] |= 229376u;
	},
//  for an occlusion at [15,14]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16776704u;
		shadow[1,0] |= 16776704u;
		shadow[2,0] |= 8387584u;
		shadow[3,0] |= 8387584u;
		shadow[4,0] |= 4192256u;
		shadow[5,0] |= 4192256u;
		shadow[6,0] |= 2093056u;
		shadow[7,0] |= 2093056u;
		shadow[8,0] |= 1040384u;
		shadow[9,0] |= 1040384u;
		shadow[10,0] |= 507904u;
		shadow[11,0] |= 507904u;
		shadow[12,0] |= 229376u;
		shadow[13,0] |= 229376u;
	},
//  for an occlusion at [15,15]:
    null,
//  for an occlusion at [15,16]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 229376u;
		shadow[18,0] |= 229376u;
		shadow[19,0] |= 507904u;
		shadow[20,0] |= 507904u;
		shadow[21,0] |= 1040384u;
		shadow[22,0] |= 1040384u;
		shadow[23,0] |= 2093056u;
		shadow[24,0] |= 2093056u;
		shadow[25,0] |= 4192256u;
		shadow[26,0] |= 4192256u;
		shadow[27,0] |= 8387584u;
		shadow[28,0] |= 8387584u;
		shadow[29,0] |= 16776704u;
		shadow[30,0] |= 16776704u;
	},
//  for an occlusion at [15,17]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 229376u;
		shadow[19,0] |= 229376u;
		shadow[20,0] |= 229376u;
		shadow[21,0] |= 229376u;
		shadow[22,0] |= 507904u;
		shadow[23,0] |= 507904u;
		shadow[24,0] |= 507904u;
		shadow[25,0] |= 507904u;
		shadow[26,0] |= 1040384u;
		shadow[27,0] |= 1040384u;
		shadow[28,0] |= 1040384u;
		shadow[29,0] |= 1040384u;
		shadow[30,0] |= 1040384u;
	},
//  for an occlusion at [15,18]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 229376u;
		shadow[20,0] |= 229376u;
		shadow[21,0] |= 229376u;
		shadow[22,0] |= 229376u;
		shadow[23,0] |= 229376u;
		shadow[24,0] |= 229376u;
		shadow[25,0] |= 507904u;
		shadow[26,0] |= 507904u;
		shadow[27,0] |= 507904u;
		shadow[28,0] |= 507904u;
		shadow[29,0] |= 507904u;
		shadow[30,0] |= 507904u;
	},
//  for an occlusion at [15,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 229376u;
		shadow[21,0] |= 229376u;
		shadow[22,0] |= 229376u;
		shadow[23,0] |= 229376u;
		shadow[24,0] |= 229376u;
		shadow[25,0] |= 229376u;
		shadow[26,0] |= 229376u;
		shadow[27,0] |= 229376u;
		shadow[28,0] |= 229376u;
		shadow[29,0] |= 229376u;
		shadow[30,0] |= 229376u;
	},
//  for an occlusion at [15,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 229376u;
		shadow[22,0] |= 229376u;
		shadow[23,0] |= 229376u;
		shadow[24,0] |= 229376u;
		shadow[25,0] |= 229376u;
		shadow[26,0] |= 229376u;
		shadow[27,0] |= 229376u;
		shadow[28,0] |= 229376u;
		shadow[29,0] |= 229376u;
		shadow[30,0] |= 229376u;
	},
//  for an occlusion at [15,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 229376u;
		shadow[23,0] |= 229376u;
		shadow[24,0] |= 229376u;
		shadow[25,0] |= 229376u;
		shadow[26,0] |= 229376u;
		shadow[27,0] |= 229376u;
		shadow[28,0] |= 229376u;
		shadow[29,0] |= 229376u;
		shadow[30,0] |= 229376u;
	},
//  for an occlusion at [15,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 229376u;
		shadow[24,0] |= 229376u;
		shadow[25,0] |= 229376u;
		shadow[26,0] |= 229376u;
		shadow[27,0] |= 229376u;
		shadow[28,0] |= 229376u;
		shadow[29,0] |= 229376u;
		shadow[30,0] |= 229376u;
	},
//  for an occlusion at [15,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 65536u;
		shadow[25,0] |= 65536u;
		shadow[26,0] |= 65536u;
		shadow[27,0] |= 65536u;
		shadow[28,0] |= 65536u;
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 65536u;
		shadow[26,0] |= 65536u;
		shadow[27,0] |= 65536u;
		shadow[28,0] |= 65536u;
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 65536u;
		shadow[27,0] |= 65536u;
		shadow[28,0] |= 65536u;
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 65536u;
		shadow[28,0] |= 65536u;
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 65536u;
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 65536u;
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 65536u;
	},
//  for an occlusion at [15,30]:
    null,
  },
  {
//  for an occlusion at [16,0]:
    null,
//  for an occlusion at [16,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32768u;
	},
//  for an occlusion at [16,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32768u;
		shadow[1,0] |= 32768u;
	},
//  for an occlusion at [16,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32768u;
		shadow[1,0] |= 32768u;
		shadow[2,0] |= 32768u;
	},
//  for an occlusion at [16,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 49152u;
		shadow[1,0] |= 49152u;
		shadow[2,0] |= 49152u;
		shadow[3,0] |= 49152u;
	},
//  for an occlusion at [16,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 49152u;
		shadow[1,0] |= 49152u;
		shadow[2,0] |= 49152u;
		shadow[3,0] |= 49152u;
		shadow[4,0] |= 49152u;
	},
//  for an occlusion at [16,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 49152u;
		shadow[1,0] |= 49152u;
		shadow[2,0] |= 49152u;
		shadow[3,0] |= 49152u;
		shadow[4,0] |= 49152u;
		shadow[5,0] |= 49152u;
	},
//  for an occlusion at [16,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 49152u;
		shadow[1,0] |= 49152u;
		shadow[2,0] |= 49152u;
		shadow[3,0] |= 49152u;
		shadow[4,0] |= 49152u;
		shadow[5,0] |= 49152u;
		shadow[6,0] |= 49152u;
	},
//  for an occlusion at [16,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24576u;
		shadow[1,0] |= 24576u;
		shadow[2,0] |= 57344u;
		shadow[3,0] |= 57344u;
		shadow[4,0] |= 49152u;
		shadow[5,0] |= 49152u;
		shadow[6,0] |= 49152u;
		shadow[7,0] |= 49152u;
	},
//  for an occlusion at [16,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24576u;
		shadow[1,0] |= 24576u;
		shadow[2,0] |= 24576u;
		shadow[3,0] |= 24576u;
		shadow[4,0] |= 57344u;
		shadow[5,0] |= 49152u;
		shadow[6,0] |= 49152u;
		shadow[7,0] |= 49152u;
		shadow[8,0] |= 49152u;
	},
//  for an occlusion at [16,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 28672u;
		shadow[1,0] |= 28672u;
		shadow[2,0] |= 28672u;
		shadow[3,0] |= 24576u;
		shadow[4,0] |= 57344u;
		shadow[5,0] |= 57344u;
		shadow[6,0] |= 57344u;
		shadow[7,0] |= 49152u;
		shadow[8,0] |= 49152u;
		shadow[9,0] |= 49152u;
	},
//  for an occlusion at [16,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 30720u;
		shadow[1,0] |= 30720u;
		shadow[2,0] |= 30720u;
		shadow[3,0] |= 28672u;
		shadow[4,0] |= 61440u;
		shadow[5,0] |= 61440u;
		shadow[6,0] |= 57344u;
		shadow[7,0] |= 57344u;
		shadow[8,0] |= 57344u;
		shadow[9,0] |= 49152u;
		shadow[10,0] |= 49152u;
	},
//  for an occlusion at [16,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 15872u;
		shadow[1,0] |= 15872u;
		shadow[2,0] |= 15360u;
		shadow[3,0] |= 31744u;
		shadow[4,0] |= 30720u;
		shadow[5,0] |= 30720u;
		shadow[6,0] |= 28672u;
		shadow[7,0] |= 61440u;
		shadow[8,0] |= 57344u;
		shadow[9,0] |= 57344u;
		shadow[10,0] |= 49152u;
		shadow[11,0] |= 49152u;
	},
//  for an occlusion at [16,13]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8160u;
		shadow[1,0] |= 8128u;
		shadow[2,0] |= 16320u;
		shadow[3,0] |= 16256u;
		shadow[4,0] |= 16128u;
		shadow[5,0] |= 15872u;
		shadow[6,0] |= 32256u;
		shadow[7,0] |= 31744u;
		shadow[8,0] |= 30720u;
		shadow[9,0] |= 28672u;
		shadow[10,0] |= 61440u;
		shadow[11,0] |= 57344u;
		shadow[12,0] |= 49152u;
	},
//  for an occlusion at [16,14]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 510u;
		shadow[1,0] |= 1022u;
		shadow[2,0] |= 1022u;
		shadow[3,0] |= 2046u;
		shadow[4,0] |= 2046u;
		shadow[5,0] |= 4094u;
		shadow[6,0] |= 4094u;
		shadow[7,0] |= 8190u;
		shadow[8,0] |= 8188u;
		shadow[9,0] |= 16368u;
		shadow[10,0] |= 16320u;
		shadow[11,0] |= 32512u;
		shadow[12,0] |= 31744u;
		shadow[13,0] |= 61440u;
		shadow[14,0] |= 16384u;
	},
//  for an occlusion at [16,15]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 30u;
		shadow[10,0] |= 126u;
		shadow[11,0] |= 510u;
		shadow[12,0] |= 2046u;
		shadow[13,0] |= 8190u;
		shadow[14,0] |= 32766u;
		shadow[15,0] |= 32766u;
		shadow[16,0] |= 32766u;
		shadow[17,0] |= 8190u;
		shadow[18,0] |= 2046u;
		shadow[19,0] |= 510u;
		shadow[20,0] |= 126u;
		shadow[21,0] |= 30u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [16,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 16384u;
		shadow[17,0] |= 61440u;
		shadow[18,0] |= 31744u;
		shadow[19,0] |= 32512u;
		shadow[20,0] |= 16320u;
		shadow[21,0] |= 16368u;
		shadow[22,0] |= 8188u;
		shadow[23,0] |= 8190u;
		shadow[24,0] |= 4094u;
		shadow[25,0] |= 4094u;
		shadow[26,0] |= 2046u;
		shadow[27,0] |= 2046u;
		shadow[28,0] |= 1022u;
		shadow[29,0] |= 1022u;
		shadow[30,0] |= 510u;
	},
//  for an occlusion at [16,17]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 49152u;
		shadow[19,0] |= 57344u;
		shadow[20,0] |= 61440u;
		shadow[21,0] |= 28672u;
		shadow[22,0] |= 30720u;
		shadow[23,0] |= 31744u;
		shadow[24,0] |= 32256u;
		shadow[25,0] |= 15872u;
		shadow[26,0] |= 16128u;
		shadow[27,0] |= 16256u;
		shadow[28,0] |= 16320u;
		shadow[29,0] |= 8128u;
		shadow[30,0] |= 8160u;
	},
//  for an occlusion at [16,18]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 49152u;
		shadow[20,0] |= 49152u;
		shadow[21,0] |= 57344u;
		shadow[22,0] |= 57344u;
		shadow[23,0] |= 61440u;
		shadow[24,0] |= 28672u;
		shadow[25,0] |= 30720u;
		shadow[26,0] |= 30720u;
		shadow[27,0] |= 31744u;
		shadow[28,0] |= 15360u;
		shadow[29,0] |= 15872u;
		shadow[30,0] |= 15872u;
	},
//  for an occlusion at [16,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 49152u;
		shadow[21,0] |= 49152u;
		shadow[22,0] |= 57344u;
		shadow[23,0] |= 57344u;
		shadow[24,0] |= 57344u;
		shadow[25,0] |= 61440u;
		shadow[26,0] |= 61440u;
		shadow[27,0] |= 28672u;
		shadow[28,0] |= 30720u;
		shadow[29,0] |= 30720u;
		shadow[30,0] |= 30720u;
	},
//  for an occlusion at [16,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 49152u;
		shadow[22,0] |= 49152u;
		shadow[23,0] |= 49152u;
		shadow[24,0] |= 57344u;
		shadow[25,0] |= 57344u;
		shadow[26,0] |= 57344u;
		shadow[27,0] |= 24576u;
		shadow[28,0] |= 28672u;
		shadow[29,0] |= 28672u;
		shadow[30,0] |= 28672u;
	},
//  for an occlusion at [16,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 49152u;
		shadow[23,0] |= 49152u;
		shadow[24,0] |= 49152u;
		shadow[25,0] |= 49152u;
		shadow[26,0] |= 57344u;
		shadow[27,0] |= 24576u;
		shadow[28,0] |= 24576u;
		shadow[29,0] |= 24576u;
		shadow[30,0] |= 24576u;
	},
//  for an occlusion at [16,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 49152u;
		shadow[24,0] |= 49152u;
		shadow[25,0] |= 49152u;
		shadow[26,0] |= 49152u;
		shadow[27,0] |= 57344u;
		shadow[28,0] |= 57344u;
		shadow[29,0] |= 24576u;
		shadow[30,0] |= 24576u;
	},
//  for an occlusion at [16,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 49152u;
		shadow[25,0] |= 49152u;
		shadow[26,0] |= 49152u;
		shadow[27,0] |= 49152u;
		shadow[28,0] |= 49152u;
		shadow[29,0] |= 49152u;
		shadow[30,0] |= 49152u;
	},
//  for an occlusion at [16,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 49152u;
		shadow[26,0] |= 49152u;
		shadow[27,0] |= 49152u;
		shadow[28,0] |= 49152u;
		shadow[29,0] |= 49152u;
		shadow[30,0] |= 49152u;
	},
//  for an occlusion at [16,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 49152u;
		shadow[27,0] |= 49152u;
		shadow[28,0] |= 49152u;
		shadow[29,0] |= 49152u;
		shadow[30,0] |= 49152u;
	},
//  for an occlusion at [16,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 49152u;
		shadow[28,0] |= 49152u;
		shadow[29,0] |= 49152u;
		shadow[30,0] |= 49152u;
	},
//  for an occlusion at [16,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 32768u;
		shadow[29,0] |= 32768u;
		shadow[30,0] |= 32768u;
	},
//  for an occlusion at [16,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 32768u;
		shadow[30,0] |= 32768u;
	},
//  for an occlusion at [16,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 32768u;
	},
//  for an occlusion at [16,30]:
    null,
  },
  {
//  for an occlusion at [17,0]:
    null,
//  for an occlusion at [17,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16384u;
	},
//  for an occlusion at [17,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16384u;
		shadow[1,0] |= 16384u;
	},
//  for an occlusion at [17,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24576u;
		shadow[1,0] |= 24576u;
		shadow[2,0] |= 24576u;
	},
//  for an occlusion at [17,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8192u;
		shadow[1,0] |= 24576u;
		shadow[2,0] |= 24576u;
		shadow[3,0] |= 24576u;
	},
//  for an occlusion at [17,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8192u;
		shadow[1,0] |= 8192u;
		shadow[2,0] |= 24576u;
		shadow[3,0] |= 24576u;
		shadow[4,0] |= 24576u;
	},
//  for an occlusion at [17,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12288u;
		shadow[1,0] |= 12288u;
		shadow[2,0] |= 8192u;
		shadow[3,0] |= 24576u;
		shadow[4,0] |= 24576u;
		shadow[5,0] |= 24576u;
	},
//  for an occlusion at [17,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12288u;
		shadow[1,0] |= 12288u;
		shadow[2,0] |= 12288u;
		shadow[3,0] |= 28672u;
		shadow[4,0] |= 24576u;
		shadow[5,0] |= 24576u;
		shadow[6,0] |= 24576u;
	},
//  for an occlusion at [17,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6144u;
		shadow[1,0] |= 6144u;
		shadow[2,0] |= 14336u;
		shadow[3,0] |= 12288u;
		shadow[4,0] |= 12288u;
		shadow[5,0] |= 28672u;
		shadow[6,0] |= 24576u;
		shadow[7,0] |= 24576u;
	},
//  for an occlusion at [17,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 7168u;
		shadow[1,0] |= 7168u;
		shadow[2,0] |= 14336u;
		shadow[3,0] |= 14336u;
		shadow[4,0] |= 14336u;
		shadow[5,0] |= 12288u;
		shadow[6,0] |= 28672u;
		shadow[7,0] |= 24576u;
		shadow[8,0] |= 24576u;
	},
//  for an occlusion at [17,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3584u;
		shadow[1,0] |= 3584u;
		shadow[2,0] |= 7168u;
		shadow[3,0] |= 7168u;
		shadow[4,0] |= 14336u;
		shadow[5,0] |= 14336u;
		shadow[6,0] |= 12288u;
		shadow[7,0] |= 28672u;
		shadow[8,0] |= 24576u;
		shadow[9,0] |= 24576u;
	},
//  for an occlusion at [17,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1920u;
		shadow[1,0] |= 1792u;
		shadow[2,0] |= 3840u;
		shadow[3,0] |= 3584u;
		shadow[4,0] |= 7680u;
		shadow[5,0] |= 7168u;
		shadow[6,0] |= 15360u;
		shadow[7,0] |= 14336u;
		shadow[8,0] |= 12288u;
		shadow[9,0] |= 28672u;
		shadow[10,0] |= 24576u;
	},
//  for an occlusion at [17,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 496u;
		shadow[1,0] |= 992u;
		shadow[2,0] |= 960u;
		shadow[3,0] |= 1984u;
		shadow[4,0] |= 1920u;
		shadow[5,0] |= 3840u;
		shadow[6,0] |= 3584u;
		shadow[7,0] |= 7680u;
		shadow[8,0] |= 7168u;
		shadow[9,0] |= 14336u;
		shadow[10,0] |= 12288u;
		shadow[11,0] |= 24576u;
	},
//  for an occlusion at [17,13]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 30u;
		shadow[1,0] |= 62u;
		shadow[2,0] |= 126u;
		shadow[3,0] |= 254u;
		shadow[4,0] |= 508u;
		shadow[5,0] |= 504u;
		shadow[6,0] |= 1008u;
		shadow[7,0] |= 2016u;
		shadow[8,0] |= 3968u;
		shadow[9,0] |= 3840u;
		shadow[10,0] |= 7680u;
		shadow[11,0] |= 14336u;
		shadow[12,0] |= 28672u;
		shadow[13,0] |= 8192u;
	},
//  for an occlusion at [17,14]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2u;
		shadow[5,0] |= 14u;
		shadow[6,0] |= 30u;
		shadow[7,0] |= 62u;
		shadow[8,0] |= 254u;
		shadow[9,0] |= 510u;
		shadow[10,0] |= 1022u;
		shadow[11,0] |= 4094u;
		shadow[12,0] |= 8184u;
		shadow[13,0] |= 16256u;
		shadow[14,0] |= 14336u;
	},
//  for an occlusion at [17,15]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 62u;
		shadow[13,0] |= 1022u;
		shadow[14,0] |= 16382u;
		shadow[15,0] |= 16382u;
		shadow[16,0] |= 16382u;
		shadow[17,0] |= 1022u;
		shadow[18,0] |= 62u;
	},
//  for an occlusion at [17,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 14336u;
		shadow[17,0] |= 16256u;
		shadow[18,0] |= 8184u;
		shadow[19,0] |= 4094u;
		shadow[20,0] |= 1022u;
		shadow[21,0] |= 510u;
		shadow[22,0] |= 254u;
		shadow[23,0] |= 62u;
		shadow[24,0] |= 30u;
		shadow[25,0] |= 14u;
		shadow[26,0] |= 2u;
	},
//  for an occlusion at [17,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 8192u;
		shadow[18,0] |= 28672u;
		shadow[19,0] |= 14336u;
		shadow[20,0] |= 7680u;
		shadow[21,0] |= 3840u;
		shadow[22,0] |= 3968u;
		shadow[23,0] |= 2016u;
		shadow[24,0] |= 1008u;
		shadow[25,0] |= 504u;
		shadow[26,0] |= 508u;
		shadow[27,0] |= 254u;
		shadow[28,0] |= 126u;
		shadow[29,0] |= 62u;
		shadow[30,0] |= 30u;
	},
//  for an occlusion at [17,18]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 24576u;
		shadow[20,0] |= 12288u;
		shadow[21,0] |= 14336u;
		shadow[22,0] |= 7168u;
		shadow[23,0] |= 7680u;
		shadow[24,0] |= 3584u;
		shadow[25,0] |= 3840u;
		shadow[26,0] |= 1920u;
		shadow[27,0] |= 1984u;
		shadow[28,0] |= 960u;
		shadow[29,0] |= 992u;
		shadow[30,0] |= 496u;
	},
//  for an occlusion at [17,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 24576u;
		shadow[21,0] |= 28672u;
		shadow[22,0] |= 12288u;
		shadow[23,0] |= 14336u;
		shadow[24,0] |= 15360u;
		shadow[25,0] |= 7168u;
		shadow[26,0] |= 7680u;
		shadow[27,0] |= 3584u;
		shadow[28,0] |= 3840u;
		shadow[29,0] |= 1792u;
		shadow[30,0] |= 1920u;
	},
//  for an occlusion at [17,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 24576u;
		shadow[22,0] |= 24576u;
		shadow[23,0] |= 28672u;
		shadow[24,0] |= 12288u;
		shadow[25,0] |= 14336u;
		shadow[26,0] |= 14336u;
		shadow[27,0] |= 7168u;
		shadow[28,0] |= 7168u;
		shadow[29,0] |= 3584u;
		shadow[30,0] |= 3584u;
	},
//  for an occlusion at [17,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 24576u;
		shadow[23,0] |= 24576u;
		shadow[24,0] |= 28672u;
		shadow[25,0] |= 12288u;
		shadow[26,0] |= 14336u;
		shadow[27,0] |= 14336u;
		shadow[28,0] |= 14336u;
		shadow[29,0] |= 7168u;
		shadow[30,0] |= 7168u;
	},
//  for an occlusion at [17,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 24576u;
		shadow[24,0] |= 24576u;
		shadow[25,0] |= 28672u;
		shadow[26,0] |= 12288u;
		shadow[27,0] |= 12288u;
		shadow[28,0] |= 14336u;
		shadow[29,0] |= 6144u;
		shadow[30,0] |= 6144u;
	},
//  for an occlusion at [17,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 24576u;
		shadow[25,0] |= 24576u;
		shadow[26,0] |= 24576u;
		shadow[27,0] |= 28672u;
		shadow[28,0] |= 12288u;
		shadow[29,0] |= 12288u;
		shadow[30,0] |= 12288u;
	},
//  for an occlusion at [17,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 24576u;
		shadow[26,0] |= 24576u;
		shadow[27,0] |= 24576u;
		shadow[28,0] |= 8192u;
		shadow[29,0] |= 12288u;
		shadow[30,0] |= 12288u;
	},
//  for an occlusion at [17,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 24576u;
		shadow[27,0] |= 24576u;
		shadow[28,0] |= 24576u;
		shadow[29,0] |= 8192u;
		shadow[30,0] |= 8192u;
	},
//  for an occlusion at [17,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 24576u;
		shadow[28,0] |= 24576u;
		shadow[29,0] |= 24576u;
		shadow[30,0] |= 8192u;
	},
//  for an occlusion at [17,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 24576u;
		shadow[29,0] |= 24576u;
		shadow[30,0] |= 24576u;
	},
//  for an occlusion at [17,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 16384u;
		shadow[30,0] |= 16384u;
	},
//  for an occlusion at [17,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 16384u;
	},
//  for an occlusion at [17,30]:
    null,
  },
  {
//  for an occlusion at [18,0]:
    null,
//  for an occlusion at [18,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8192u;
	},
//  for an occlusion at [18,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12288u;
		shadow[1,0] |= 12288u;
	},
//  for an occlusion at [18,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4096u;
		shadow[1,0] |= 12288u;
		shadow[2,0] |= 12288u;
	},
//  for an occlusion at [18,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4096u;
		shadow[1,0] |= 4096u;
		shadow[2,0] |= 12288u;
		shadow[3,0] |= 12288u;
	},
//  for an occlusion at [18,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6144u;
		shadow[1,0] |= 6144u;
		shadow[2,0] |= 12288u;
		shadow[3,0] |= 12288u;
		shadow[4,0] |= 12288u;
	},
//  for an occlusion at [18,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2048u;
		shadow[1,0] |= 6144u;
		shadow[2,0] |= 6144u;
		shadow[3,0] |= 14336u;
		shadow[4,0] |= 12288u;
		shadow[5,0] |= 12288u;
	},
//  for an occlusion at [18,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3072u;
		shadow[1,0] |= 3072u;
		shadow[2,0] |= 7168u;
		shadow[3,0] |= 6144u;
		shadow[4,0] |= 6144u;
		shadow[5,0] |= 12288u;
		shadow[6,0] |= 12288u;
	},
//  for an occlusion at [18,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1536u;
		shadow[1,0] |= 1536u;
		shadow[2,0] |= 3072u;
		shadow[3,0] |= 3072u;
		shadow[4,0] |= 6144u;
		shadow[5,0] |= 6144u;
		shadow[6,0] |= 12288u;
		shadow[7,0] |= 12288u;
	},
//  for an occlusion at [18,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 768u;
		shadow[1,0] |= 1792u;
		shadow[2,0] |= 1536u;
		shadow[3,0] |= 3584u;
		shadow[4,0] |= 3072u;
		shadow[5,0] |= 7168u;
		shadow[6,0] |= 6144u;
		shadow[7,0] |= 14336u;
		shadow[8,0] |= 12288u;
	},
//  for an occlusion at [18,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 448u;
		shadow[1,0] |= 896u;
		shadow[2,0] |= 896u;
		shadow[3,0] |= 1792u;
		shadow[4,0] |= 1792u;
		shadow[5,0] |= 3584u;
		shadow[6,0] |= 3072u;
		shadow[7,0] |= 7168u;
		shadow[8,0] |= 6144u;
		shadow[9,0] |= 12288u;
	},
//  for an occlusion at [18,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 120u;
		shadow[1,0] |= 240u;
		shadow[2,0] |= 480u;
		shadow[3,0] |= 448u;
		shadow[4,0] |= 960u;
		shadow[5,0] |= 1920u;
		shadow[6,0] |= 1792u;
		shadow[7,0] |= 3584u;
		shadow[8,0] |= 7168u;
		shadow[9,0] |= 6144u;
		shadow[10,0] |= 12288u;
	},
//  for an occlusion at [18,12]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 14u;
		shadow[1,0] |= 30u;
		shadow[2,0] |= 62u;
		shadow[3,0] |= 124u;
		shadow[4,0] |= 248u;
		shadow[5,0] |= 496u;
		shadow[6,0] |= 480u;
		shadow[7,0] |= 960u;
		shadow[8,0] |= 1792u;
		shadow[9,0] |= 3584u;
		shadow[10,0] |= 7168u;
		shadow[11,0] |= 14336u;
		shadow[12,0] |= 4096u;
	},
//  for an occlusion at [18,13]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
		shadow[4,0] |= 6u;
		shadow[5,0] |= 30u;
		shadow[6,0] |= 62u;
		shadow[7,0] |= 126u;
		shadow[8,0] |= 508u;
		shadow[9,0] |= 1008u;
		shadow[10,0] |= 1984u;
		shadow[11,0] |= 3840u;
		shadow[12,0] |= 7168u;
		shadow[13,0] |= 4096u;
	},
//  for an occlusion at [18,14]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 30u;
		shadow[10,0] |= 126u;
		shadow[11,0] |= 510u;
		shadow[12,0] |= 2046u;
		shadow[13,0] |= 8176u;
		shadow[14,0] |= 7936u;
	},
//  for an occlusion at [18,15]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 126u;
		shadow[14,0] |= 8190u;
		shadow[15,0] |= 8190u;
		shadow[16,0] |= 8190u;
		shadow[17,0] |= 126u;
	},
//  for an occlusion at [18,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 7936u;
		shadow[17,0] |= 8176u;
		shadow[18,0] |= 2046u;
		shadow[19,0] |= 510u;
		shadow[20,0] |= 126u;
		shadow[21,0] |= 30u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [18,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 4096u;
		shadow[18,0] |= 7168u;
		shadow[19,0] |= 3840u;
		shadow[20,0] |= 1984u;
		shadow[21,0] |= 1008u;
		shadow[22,0] |= 508u;
		shadow[23,0] |= 126u;
		shadow[24,0] |= 62u;
		shadow[25,0] |= 30u;
		shadow[26,0] |= 6u;
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [18,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 4096u;
		shadow[19,0] |= 14336u;
		shadow[20,0] |= 7168u;
		shadow[21,0] |= 3584u;
		shadow[22,0] |= 1792u;
		shadow[23,0] |= 960u;
		shadow[24,0] |= 480u;
		shadow[25,0] |= 496u;
		shadow[26,0] |= 248u;
		shadow[27,0] |= 124u;
		shadow[28,0] |= 62u;
		shadow[29,0] |= 30u;
		shadow[30,0] |= 14u;
	},
//  for an occlusion at [18,19]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 12288u;
		shadow[21,0] |= 6144u;
		shadow[22,0] |= 7168u;
		shadow[23,0] |= 3584u;
		shadow[24,0] |= 1792u;
		shadow[25,0] |= 1920u;
		shadow[26,0] |= 960u;
		shadow[27,0] |= 448u;
		shadow[28,0] |= 480u;
		shadow[29,0] |= 240u;
		shadow[30,0] |= 120u;
	},
//  for an occlusion at [18,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 12288u;
		shadow[22,0] |= 6144u;
		shadow[23,0] |= 7168u;
		shadow[24,0] |= 3072u;
		shadow[25,0] |= 3584u;
		shadow[26,0] |= 1792u;
		shadow[27,0] |= 1792u;
		shadow[28,0] |= 896u;
		shadow[29,0] |= 896u;
		shadow[30,0] |= 448u;
	},
//  for an occlusion at [18,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 12288u;
		shadow[23,0] |= 14336u;
		shadow[24,0] |= 6144u;
		shadow[25,0] |= 7168u;
		shadow[26,0] |= 3072u;
		shadow[27,0] |= 3584u;
		shadow[28,0] |= 1536u;
		shadow[29,0] |= 1792u;
		shadow[30,0] |= 768u;
	},
//  for an occlusion at [18,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 12288u;
		shadow[24,0] |= 12288u;
		shadow[25,0] |= 6144u;
		shadow[26,0] |= 6144u;
		shadow[27,0] |= 3072u;
		shadow[28,0] |= 3072u;
		shadow[29,0] |= 1536u;
		shadow[30,0] |= 1536u;
	},
//  for an occlusion at [18,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 12288u;
		shadow[25,0] |= 12288u;
		shadow[26,0] |= 6144u;
		shadow[27,0] |= 6144u;
		shadow[28,0] |= 7168u;
		shadow[29,0] |= 3072u;
		shadow[30,0] |= 3072u;
	},
//  for an occlusion at [18,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 12288u;
		shadow[26,0] |= 12288u;
		shadow[27,0] |= 14336u;
		shadow[28,0] |= 6144u;
		shadow[29,0] |= 6144u;
		shadow[30,0] |= 2048u;
	},
//  for an occlusion at [18,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 12288u;
		shadow[27,0] |= 12288u;
		shadow[28,0] |= 12288u;
		shadow[29,0] |= 6144u;
		shadow[30,0] |= 6144u;
	},
//  for an occlusion at [18,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 12288u;
		shadow[28,0] |= 12288u;
		shadow[29,0] |= 4096u;
		shadow[30,0] |= 4096u;
	},
//  for an occlusion at [18,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 12288u;
		shadow[29,0] |= 12288u;
		shadow[30,0] |= 4096u;
	},
//  for an occlusion at [18,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 12288u;
		shadow[30,0] |= 12288u;
	},
//  for an occlusion at [18,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 8192u;
	},
//  for an occlusion at [18,30]:
    null,
  },
  {
//  for an occlusion at [19,0]:
    null,
//  for an occlusion at [19,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4096u;
	},
//  for an occlusion at [19,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2048u;
		shadow[1,0] |= 6144u;
	},
//  for an occlusion at [19,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2048u;
		shadow[1,0] |= 2048u;
		shadow[2,0] |= 6144u;
	},
//  for an occlusion at [19,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 3072u;
		shadow[1,0] |= 3072u;
		shadow[2,0] |= 6144u;
		shadow[3,0] |= 6144u;
	},
//  for an occlusion at [19,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1024u;
		shadow[1,0] |= 3072u;
		shadow[2,0] |= 3072u;
		shadow[3,0] |= 6144u;
		shadow[4,0] |= 6144u;
	},
//  for an occlusion at [19,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1536u;
		shadow[1,0] |= 1536u;
		shadow[2,0] |= 3072u;
		shadow[3,0] |= 3072u;
		shadow[4,0] |= 6144u;
		shadow[5,0] |= 6144u;
	},
//  for an occlusion at [19,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 768u;
		shadow[1,0] |= 512u;
		shadow[2,0] |= 1536u;
		shadow[3,0] |= 1024u;
		shadow[4,0] |= 3072u;
		shadow[5,0] |= 2048u;
		shadow[6,0] |= 6144u;
	},
//  for an occlusion at [19,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 384u;
		shadow[1,0] |= 896u;
		shadow[2,0] |= 768u;
		shadow[3,0] |= 1792u;
		shadow[4,0] |= 1536u;
		shadow[5,0] |= 3072u;
		shadow[6,0] |= 3072u;
		shadow[7,0] |= 6144u;
	},
//  for an occlusion at [19,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 224u;
		shadow[1,0] |= 448u;
		shadow[2,0] |= 448u;
		shadow[3,0] |= 896u;
		shadow[4,0] |= 768u;
		shadow[5,0] |= 1536u;
		shadow[6,0] |= 3584u;
		shadow[7,0] |= 3072u;
		shadow[8,0] |= 6144u;
	},
//  for an occlusion at [19,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 56u;
		shadow[1,0] |= 112u;
		shadow[2,0] |= 224u;
		shadow[3,0] |= 192u;
		shadow[4,0] |= 448u;
		shadow[5,0] |= 896u;
		shadow[6,0] |= 1792u;
		shadow[7,0] |= 1536u;
		shadow[8,0] |= 3072u;
		shadow[9,0] |= 6144u;
	},
//  for an occlusion at [19,11]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6u;
		shadow[1,0] |= 14u;
		shadow[2,0] |= 28u;
		shadow[3,0] |= 56u;
		shadow[4,0] |= 112u;
		shadow[5,0] |= 224u;
		shadow[6,0] |= 448u;
		shadow[7,0] |= 896u;
		shadow[8,0] |= 1792u;
		shadow[9,0] |= 3584u;
		shadow[10,0] |= 7168u;
		shadow[11,0] |= 2048u;
	},
//  for an occlusion at [19,12]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
		shadow[3,0] |= 6u;
		shadow[4,0] |= 14u;
		shadow[5,0] |= 62u;
		shadow[6,0] |= 124u;
		shadow[7,0] |= 248u;
		shadow[8,0] |= 480u;
		shadow[9,0] |= 960u;
		shadow[10,0] |= 1792u;
		shadow[11,0] |= 3584u;
		shadow[12,0] |= 2048u;
	},
//  for an occlusion at [19,13]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2u;
		shadow[7,0] |= 14u;
		shadow[8,0] |= 62u;
		shadow[9,0] |= 254u;
		shadow[10,0] |= 504u;
		shadow[11,0] |= 2016u;
		shadow[12,0] |= 3968u;
		shadow[13,0] |= 3072u;
	},
//  for an occlusion at [19,14]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 14u;
		shadow[11,0] |= 126u;
		shadow[12,0] |= 1022u;
		shadow[13,0] |= 4094u;
		shadow[14,0] |= 4064u;
	},
//  for an occlusion at [19,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 4094u;
		shadow[15,0] |= 4094u;
		shadow[16,0] |= 4094u;
	},
//  for an occlusion at [19,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 4064u;
		shadow[17,0] |= 4094u;
		shadow[18,0] |= 1022u;
		shadow[19,0] |= 126u;
		shadow[20,0] |= 14u;
	},
//  for an occlusion at [19,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 3072u;
		shadow[18,0] |= 3968u;
		shadow[19,0] |= 2016u;
		shadow[20,0] |= 504u;
		shadow[21,0] |= 254u;
		shadow[22,0] |= 62u;
		shadow[23,0] |= 14u;
		shadow[24,0] |= 2u;
	},
//  for an occlusion at [19,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 2048u;
		shadow[19,0] |= 3584u;
		shadow[20,0] |= 1792u;
		shadow[21,0] |= 960u;
		shadow[22,0] |= 480u;
		shadow[23,0] |= 248u;
		shadow[24,0] |= 124u;
		shadow[25,0] |= 62u;
		shadow[26,0] |= 14u;
		shadow[27,0] |= 6u;
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [19,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 2048u;
		shadow[20,0] |= 7168u;
		shadow[21,0] |= 3584u;
		shadow[22,0] |= 1792u;
		shadow[23,0] |= 896u;
		shadow[24,0] |= 448u;
		shadow[25,0] |= 224u;
		shadow[26,0] |= 112u;
		shadow[27,0] |= 56u;
		shadow[28,0] |= 28u;
		shadow[29,0] |= 14u;
		shadow[30,0] |= 6u;
	},
//  for an occlusion at [19,20]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 6144u;
		shadow[22,0] |= 3072u;
		shadow[23,0] |= 1536u;
		shadow[24,0] |= 1792u;
		shadow[25,0] |= 896u;
		shadow[26,0] |= 448u;
		shadow[27,0] |= 192u;
		shadow[28,0] |= 224u;
		shadow[29,0] |= 112u;
		shadow[30,0] |= 56u;
	},
//  for an occlusion at [19,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 6144u;
		shadow[23,0] |= 3072u;
		shadow[24,0] |= 3584u;
		shadow[25,0] |= 1536u;
		shadow[26,0] |= 768u;
		shadow[27,0] |= 896u;
		shadow[28,0] |= 448u;
		shadow[29,0] |= 448u;
		shadow[30,0] |= 224u;
	},
//  for an occlusion at [19,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 6144u;
		shadow[24,0] |= 3072u;
		shadow[25,0] |= 3072u;
		shadow[26,0] |= 1536u;
		shadow[27,0] |= 1792u;
		shadow[28,0] |= 768u;
		shadow[29,0] |= 896u;
		shadow[30,0] |= 384u;
	},
//  for an occlusion at [19,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 6144u;
		shadow[25,0] |= 2048u;
		shadow[26,0] |= 3072u;
		shadow[27,0] |= 1024u;
		shadow[28,0] |= 1536u;
		shadow[29,0] |= 512u;
		shadow[30,0] |= 768u;
	},
//  for an occlusion at [19,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 6144u;
		shadow[26,0] |= 6144u;
		shadow[27,0] |= 3072u;
		shadow[28,0] |= 3072u;
		shadow[29,0] |= 1536u;
		shadow[30,0] |= 1536u;
	},
//  for an occlusion at [19,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 6144u;
		shadow[27,0] |= 6144u;
		shadow[28,0] |= 3072u;
		shadow[29,0] |= 3072u;
		shadow[30,0] |= 1024u;
	},
//  for an occlusion at [19,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 6144u;
		shadow[28,0] |= 6144u;
		shadow[29,0] |= 3072u;
		shadow[30,0] |= 3072u;
	},
//  for an occlusion at [19,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 6144u;
		shadow[29,0] |= 2048u;
		shadow[30,0] |= 2048u;
	},
//  for an occlusion at [19,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 6144u;
		shadow[30,0] |= 2048u;
	},
//  for an occlusion at [19,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 4096u;
	},
//  for an occlusion at [19,30]:
    null,
  },
  {
//  for an occlusion at [20,0]:
    null,
//  for an occlusion at [20,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2048u;
	},
//  for an occlusion at [20,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1024u;
		shadow[1,0] |= 3072u;
	},
//  for an occlusion at [20,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1024u;
		shadow[1,0] |= 1024u;
		shadow[2,0] |= 3072u;
	},
//  for an occlusion at [20,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 512u;
		shadow[1,0] |= 1536u;
		shadow[2,0] |= 1024u;
		shadow[3,0] |= 3072u;
	},
//  for an occlusion at [20,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 768u;
		shadow[1,0] |= 512u;
		shadow[2,0] |= 1536u;
		shadow[3,0] |= 1024u;
		shadow[4,0] |= 3072u;
	},
//  for an occlusion at [20,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 384u;
		shadow[1,0] |= 768u;
		shadow[2,0] |= 768u;
		shadow[3,0] |= 1536u;
		shadow[4,0] |= 1536u;
		shadow[5,0] |= 3072u;
	},
//  for an occlusion at [20,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 192u;
		shadow[1,0] |= 384u;
		shadow[2,0] |= 896u;
		shadow[3,0] |= 768u;
		shadow[4,0] |= 1536u;
		shadow[5,0] |= 1536u;
		shadow[6,0] |= 3072u;
	},
//  for an occlusion at [20,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 96u;
		shadow[1,0] |= 224u;
		shadow[2,0] |= 192u;
		shadow[3,0] |= 384u;
		shadow[4,0] |= 896u;
		shadow[5,0] |= 768u;
		shadow[6,0] |= 1536u;
		shadow[7,0] |= 3072u;
	},
//  for an occlusion at [20,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24u;
		shadow[1,0] |= 48u;
		shadow[2,0] |= 96u;
		shadow[3,0] |= 224u;
		shadow[4,0] |= 448u;
		shadow[5,0] |= 384u;
		shadow[6,0] |= 768u;
		shadow[7,0] |= 1536u;
		shadow[8,0] |= 3072u;
	},
//  for an occlusion at [20,10]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6u;
		shadow[1,0] |= 14u;
		shadow[2,0] |= 28u;
		shadow[3,0] |= 56u;
		shadow[4,0] |= 112u;
		shadow[5,0] |= 224u;
		shadow[6,0] |= 448u;
		shadow[7,0] |= 896u;
		shadow[8,0] |= 1792u;
		shadow[9,0] |= 3584u;
		shadow[10,0] |= 1024u;
	},
//  for an occlusion at [20,11]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
		shadow[3,0] |= 6u;
		shadow[4,0] |= 14u;
		shadow[5,0] |= 60u;
		shadow[6,0] |= 120u;
		shadow[7,0] |= 224u;
		shadow[8,0] |= 448u;
		shadow[9,0] |= 896u;
		shadow[10,0] |= 1536u;
		shadow[11,0] |= 1024u;
	},
//  for an occlusion at [20,12]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2u;
		shadow[6,0] |= 14u;
		shadow[7,0] |= 62u;
		shadow[8,0] |= 124u;
		shadow[9,0] |= 496u;
		shadow[10,0] |= 960u;
		shadow[11,0] |= 1792u;
		shadow[12,0] |= 1024u;
	},
//  for an occlusion at [20,13]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 30u;
		shadow[10,0] |= 126u;
		shadow[11,0] |= 504u;
		shadow[12,0] |= 2016u;
		shadow[13,0] |= 1792u;
	},
//  for an occlusion at [20,14]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 14u;
		shadow[12,0] |= 254u;
		shadow[13,0] |= 2046u;
		shadow[14,0] |= 2016u;
	},
//  for an occlusion at [20,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 2046u;
		shadow[15,0] |= 2046u;
		shadow[16,0] |= 2046u;
	},
//  for an occlusion at [20,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 2016u;
		shadow[17,0] |= 2046u;
		shadow[18,0] |= 254u;
		shadow[19,0] |= 14u;
	},
//  for an occlusion at [20,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 1792u;
		shadow[18,0] |= 2016u;
		shadow[19,0] |= 504u;
		shadow[20,0] |= 126u;
		shadow[21,0] |= 30u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [20,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 1024u;
		shadow[19,0] |= 1792u;
		shadow[20,0] |= 960u;
		shadow[21,0] |= 496u;
		shadow[22,0] |= 124u;
		shadow[23,0] |= 62u;
		shadow[24,0] |= 14u;
		shadow[25,0] |= 2u;
	},
//  for an occlusion at [20,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 1024u;
		shadow[20,0] |= 1536u;
		shadow[21,0] |= 896u;
		shadow[22,0] |= 448u;
		shadow[23,0] |= 224u;
		shadow[24,0] |= 120u;
		shadow[25,0] |= 60u;
		shadow[26,0] |= 14u;
		shadow[27,0] |= 6u;
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [20,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 1024u;
		shadow[21,0] |= 3584u;
		shadow[22,0] |= 1792u;
		shadow[23,0] |= 896u;
		shadow[24,0] |= 448u;
		shadow[25,0] |= 224u;
		shadow[26,0] |= 112u;
		shadow[27,0] |= 56u;
		shadow[28,0] |= 28u;
		shadow[29,0] |= 14u;
		shadow[30,0] |= 6u;
	},
//  for an occlusion at [20,21]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 3072u;
		shadow[23,0] |= 1536u;
		shadow[24,0] |= 768u;
		shadow[25,0] |= 384u;
		shadow[26,0] |= 448u;
		shadow[27,0] |= 224u;
		shadow[28,0] |= 96u;
		shadow[29,0] |= 48u;
		shadow[30,0] |= 24u;
	},
//  for an occlusion at [20,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 3072u;
		shadow[24,0] |= 1536u;
		shadow[25,0] |= 768u;
		shadow[26,0] |= 896u;
		shadow[27,0] |= 384u;
		shadow[28,0] |= 192u;
		shadow[29,0] |= 224u;
		shadow[30,0] |= 96u;
	},
//  for an occlusion at [20,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 3072u;
		shadow[25,0] |= 1536u;
		shadow[26,0] |= 1536u;
		shadow[27,0] |= 768u;
		shadow[28,0] |= 896u;
		shadow[29,0] |= 384u;
		shadow[30,0] |= 192u;
	},
//  for an occlusion at [20,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 3072u;
		shadow[26,0] |= 1536u;
		shadow[27,0] |= 1536u;
		shadow[28,0] |= 768u;
		shadow[29,0] |= 768u;
		shadow[30,0] |= 384u;
	},
//  for an occlusion at [20,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 3072u;
		shadow[27,0] |= 1024u;
		shadow[28,0] |= 1536u;
		shadow[29,0] |= 512u;
		shadow[30,0] |= 768u;
	},
//  for an occlusion at [20,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 3072u;
		shadow[28,0] |= 1024u;
		shadow[29,0] |= 1536u;
		shadow[30,0] |= 512u;
	},
//  for an occlusion at [20,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 3072u;
		shadow[29,0] |= 1024u;
		shadow[30,0] |= 1024u;
	},
//  for an occlusion at [20,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 3072u;
		shadow[30,0] |= 1024u;
	},
//  for an occlusion at [20,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 2048u;
	},
//  for an occlusion at [20,30]:
    null,
  },
  {
//  for an occlusion at [21,0]:
    null,
//  for an occlusion at [21,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 1024u;
	},
//  for an occlusion at [21,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 512u;
		shadow[1,0] |= 1536u;
	},
//  for an occlusion at [21,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 768u;
		shadow[1,0] |= 512u;
		shadow[2,0] |= 1536u;
	},
//  for an occlusion at [21,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 256u;
		shadow[1,0] |= 768u;
		shadow[2,0] |= 512u;
		shadow[3,0] |= 1536u;
	},
//  for an occlusion at [21,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 128u;
		shadow[1,0] |= 384u;
		shadow[2,0] |= 768u;
		shadow[3,0] |= 768u;
		shadow[4,0] |= 1536u;
	},
//  for an occlusion at [21,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 64u;
		shadow[1,0] |= 192u;
		shadow[2,0] |= 384u;
		shadow[3,0] |= 896u;
		shadow[4,0] |= 768u;
		shadow[5,0] |= 1536u;
	},
//  for an occlusion at [21,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 48u;
		shadow[1,0] |= 96u;
		shadow[2,0] |= 192u;
		shadow[3,0] |= 448u;
		shadow[4,0] |= 384u;
		shadow[5,0] |= 768u;
		shadow[6,0] |= 1536u;
	},
//  for an occlusion at [21,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24u;
		shadow[1,0] |= 56u;
		shadow[2,0] |= 112u;
		shadow[3,0] |= 96u;
		shadow[4,0] |= 192u;
		shadow[5,0] |= 384u;
		shadow[6,0] |= 768u;
		shadow[7,0] |= 1536u;
	},
//  for an occlusion at [21,9]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6u;
		shadow[1,0] |= 14u;
		shadow[2,0] |= 28u;
		shadow[3,0] |= 56u;
		shadow[4,0] |= 112u;
		shadow[5,0] |= 224u;
		shadow[6,0] |= 448u;
		shadow[7,0] |= 896u;
		shadow[8,0] |= 1792u;
		shadow[9,0] |= 512u;
	},
//  for an occlusion at [21,10]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
		shadow[3,0] |= 6u;
		shadow[4,0] |= 28u;
		shadow[5,0] |= 56u;
		shadow[6,0] |= 112u;
		shadow[7,0] |= 224u;
		shadow[8,0] |= 384u;
		shadow[9,0] |= 768u;
		shadow[10,0] |= 512u;
	},
//  for an occlusion at [21,11]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2u;
		shadow[5,0] |= 14u;
		shadow[6,0] |= 30u;
		shadow[7,0] |= 60u;
		shadow[8,0] |= 240u;
		shadow[9,0] |= 448u;
		shadow[10,0] |= 896u;
		shadow[11,0] |= 512u;
	},
//  for an occlusion at [21,12]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 6u;
		shadow[8,0] |= 30u;
		shadow[9,0] |= 124u;
		shadow[10,0] |= 496u;
		shadow[11,0] |= 960u;
		shadow[12,0] |= 768u;
	},
//  for an occlusion at [21,13]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 6u;
		shadow[10,0] |= 62u;
		shadow[11,0] |= 254u;
		shadow[12,0] |= 1016u;
		shadow[13,0] |= 896u;
	},
//  for an occlusion at [21,14]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 62u;
		shadow[13,0] |= 1022u;
		shadow[14,0] |= 992u;
	},
//  for an occlusion at [21,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 1022u;
		shadow[15,0] |= 1022u;
		shadow[16,0] |= 1022u;
	},
//  for an occlusion at [21,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 992u;
		shadow[17,0] |= 1022u;
		shadow[18,0] |= 62u;
	},
//  for an occlusion at [21,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 896u;
		shadow[18,0] |= 1016u;
		shadow[19,0] |= 254u;
		shadow[20,0] |= 62u;
		shadow[21,0] |= 6u;
	},
//  for an occlusion at [21,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 768u;
		shadow[19,0] |= 960u;
		shadow[20,0] |= 496u;
		shadow[21,0] |= 124u;
		shadow[22,0] |= 30u;
		shadow[23,0] |= 6u;
	},
//  for an occlusion at [21,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 512u;
		shadow[20,0] |= 896u;
		shadow[21,0] |= 448u;
		shadow[22,0] |= 240u;
		shadow[23,0] |= 60u;
		shadow[24,0] |= 30u;
		shadow[25,0] |= 14u;
		shadow[26,0] |= 2u;
	},
//  for an occlusion at [21,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 512u;
		shadow[21,0] |= 768u;
		shadow[22,0] |= 384u;
		shadow[23,0] |= 224u;
		shadow[24,0] |= 112u;
		shadow[25,0] |= 56u;
		shadow[26,0] |= 28u;
		shadow[27,0] |= 6u;
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [21,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 512u;
		shadow[22,0] |= 1792u;
		shadow[23,0] |= 896u;
		shadow[24,0] |= 448u;
		shadow[25,0] |= 224u;
		shadow[26,0] |= 112u;
		shadow[27,0] |= 56u;
		shadow[28,0] |= 28u;
		shadow[29,0] |= 14u;
		shadow[30,0] |= 6u;
	},
//  for an occlusion at [21,22]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 1536u;
		shadow[24,0] |= 768u;
		shadow[25,0] |= 384u;
		shadow[26,0] |= 192u;
		shadow[27,0] |= 96u;
		shadow[28,0] |= 112u;
		shadow[29,0] |= 56u;
		shadow[30,0] |= 24u;
	},
//  for an occlusion at [21,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 1536u;
		shadow[25,0] |= 768u;
		shadow[26,0] |= 384u;
		shadow[27,0] |= 448u;
		shadow[28,0] |= 192u;
		shadow[29,0] |= 96u;
		shadow[30,0] |= 48u;
	},
//  for an occlusion at [21,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 1536u;
		shadow[26,0] |= 768u;
		shadow[27,0] |= 896u;
		shadow[28,0] |= 384u;
		shadow[29,0] |= 192u;
		shadow[30,0] |= 64u;
	},
//  for an occlusion at [21,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 1536u;
		shadow[27,0] |= 768u;
		shadow[28,0] |= 768u;
		shadow[29,0] |= 384u;
		shadow[30,0] |= 128u;
	},
//  for an occlusion at [21,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 1536u;
		shadow[28,0] |= 512u;
		shadow[29,0] |= 768u;
		shadow[30,0] |= 256u;
	},
//  for an occlusion at [21,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 1536u;
		shadow[29,0] |= 512u;
		shadow[30,0] |= 768u;
	},
//  for an occlusion at [21,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 1536u;
		shadow[30,0] |= 512u;
	},
//  for an occlusion at [21,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 1024u;
	},
//  for an occlusion at [21,30]:
    null,
  },
  {
//  for an occlusion at [22,0]:
    null,
//  for an occlusion at [22,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 768u;
	},
//  for an occlusion at [22,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 256u;
		shadow[1,0] |= 768u;
	},
//  for an occlusion at [22,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 128u;
		shadow[1,0] |= 256u;
		shadow[2,0] |= 768u;
	},
//  for an occlusion at [22,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 192u;
		shadow[1,0] |= 384u;
		shadow[2,0] |= 384u;
		shadow[3,0] |= 768u;
	},
//  for an occlusion at [22,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 96u;
		shadow[1,0] |= 192u;
		shadow[2,0] |= 192u;
		shadow[3,0] |= 384u;
		shadow[4,0] |= 768u;
	},
//  for an occlusion at [22,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 48u;
		shadow[1,0] |= 96u;
		shadow[2,0] |= 64u;
		shadow[3,0] |= 192u;
		shadow[4,0] |= 384u;
		shadow[5,0] |= 768u;
	},
//  for an occlusion at [22,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12u;
		shadow[1,0] |= 24u;
		shadow[2,0] |= 48u;
		shadow[3,0] |= 96u;
		shadow[4,0] |= 192u;
		shadow[5,0] |= 384u;
		shadow[6,0] |= 768u;
	},
//  for an occlusion at [22,8]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 6u;
		shadow[1,0] |= 14u;
		shadow[2,0] |= 28u;
		shadow[3,0] |= 56u;
		shadow[4,0] |= 112u;
		shadow[5,0] |= 224u;
		shadow[6,0] |= 448u;
		shadow[7,0] |= 896u;
		shadow[8,0] |= 256u;
	},
//  for an occlusion at [22,9]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 6u;
		shadow[3,0] |= 14u;
		shadow[4,0] |= 28u;
		shadow[5,0] |= 56u;
		shadow[6,0] |= 96u;
		shadow[7,0] |= 192u;
		shadow[8,0] |= 384u;
		shadow[9,0] |= 256u;
	},
//  for an occlusion at [22,10]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 6u;
		shadow[5,0] |= 14u;
		shadow[6,0] |= 60u;
		shadow[7,0] |= 112u;
		shadow[8,0] |= 224u;
		shadow[9,0] |= 384u;
		shadow[10,0] |= 256u;
	},
//  for an occlusion at [22,11]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 6u;
		shadow[7,0] |= 30u;
		shadow[8,0] |= 60u;
		shadow[9,0] |= 240u;
		shadow[10,0] |= 448u;
		shadow[11,0] |= 256u;
	},
//  for an occlusion at [22,12]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 30u;
		shadow[10,0] |= 120u;
		shadow[11,0] |= 480u;
		shadow[12,0] |= 384u;
	},
//  for an occlusion at [22,13]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 14u;
		shadow[11,0] |= 126u;
		shadow[12,0] |= 504u;
		shadow[13,0] |= 448u;
	},
//  for an occlusion at [22,14]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 30u;
		shadow[13,0] |= 510u;
		shadow[14,0] |= 504u;
	},
//  for an occlusion at [22,15]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 510u;
		shadow[15,0] |= 510u;
		shadow[16,0] |= 510u;
	},
//  for an occlusion at [22,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 504u;
		shadow[17,0] |= 510u;
		shadow[18,0] |= 30u;
	},
//  for an occlusion at [22,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 448u;
		shadow[18,0] |= 504u;
		shadow[19,0] |= 126u;
		shadow[20,0] |= 14u;
	},
//  for an occlusion at [22,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 384u;
		shadow[19,0] |= 480u;
		shadow[20,0] |= 120u;
		shadow[21,0] |= 30u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [22,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 256u;
		shadow[20,0] |= 448u;
		shadow[21,0] |= 240u;
		shadow[22,0] |= 60u;
		shadow[23,0] |= 30u;
		shadow[24,0] |= 6u;
	},
//  for an occlusion at [22,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 256u;
		shadow[21,0] |= 384u;
		shadow[22,0] |= 224u;
		shadow[23,0] |= 112u;
		shadow[24,0] |= 60u;
		shadow[25,0] |= 14u;
		shadow[26,0] |= 6u;
	},
//  for an occlusion at [22,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 256u;
		shadow[22,0] |= 384u;
		shadow[23,0] |= 192u;
		shadow[24,0] |= 96u;
		shadow[25,0] |= 56u;
		shadow[26,0] |= 28u;
		shadow[27,0] |= 14u;
		shadow[28,0] |= 6u;
	},
//  for an occlusion at [22,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 256u;
		shadow[23,0] |= 896u;
		shadow[24,0] |= 448u;
		shadow[25,0] |= 224u;
		shadow[26,0] |= 112u;
		shadow[27,0] |= 56u;
		shadow[28,0] |= 28u;
		shadow[29,0] |= 14u;
		shadow[30,0] |= 6u;
	},
//  for an occlusion at [22,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 768u;
		shadow[25,0] |= 384u;
		shadow[26,0] |= 192u;
		shadow[27,0] |= 96u;
		shadow[28,0] |= 48u;
		shadow[29,0] |= 24u;
		shadow[30,0] |= 12u;
	},
//  for an occlusion at [22,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 768u;
		shadow[26,0] |= 384u;
		shadow[27,0] |= 192u;
		shadow[28,0] |= 64u;
		shadow[29,0] |= 96u;
		shadow[30,0] |= 48u;
	},
//  for an occlusion at [22,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 768u;
		shadow[27,0] |= 384u;
		shadow[28,0] |= 192u;
		shadow[29,0] |= 192u;
		shadow[30,0] |= 96u;
	},
//  for an occlusion at [22,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 768u;
		shadow[28,0] |= 384u;
		shadow[29,0] |= 384u;
		shadow[30,0] |= 192u;
	},
//  for an occlusion at [22,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 768u;
		shadow[29,0] |= 256u;
		shadow[30,0] |= 128u;
	},
//  for an occlusion at [22,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 768u;
		shadow[30,0] |= 256u;
	},
//  for an occlusion at [22,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 768u;
	},
//  for an occlusion at [22,30]:
    null,
  },
  {
//  for an occlusion at [23,0]:
    null,
//  for an occlusion at [23,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 128u;
	},
//  for an occlusion at [23,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 128u;
		shadow[1,0] |= 384u;
	},
//  for an occlusion at [23,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 64u;
		shadow[1,0] |= 128u;
		shadow[2,0] |= 384u;
	},
//  for an occlusion at [23,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32u;
		shadow[1,0] |= 64u;
		shadow[2,0] |= 192u;
		shadow[3,0] |= 384u;
	},
//  for an occlusion at [23,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16u;
		shadow[1,0] |= 32u;
		shadow[2,0] |= 96u;
		shadow[3,0] |= 192u;
		shadow[4,0] |= 384u;
	},
//  for an occlusion at [23,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12u;
		shadow[1,0] |= 24u;
		shadow[2,0] |= 48u;
		shadow[3,0] |= 96u;
		shadow[4,0] |= 192u;
		shadow[5,0] |= 384u;
	},
//  for an occlusion at [23,7]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
		shadow[2,0] |= 8u;
		shadow[3,0] |= 16u;
		shadow[4,0] |= 32u;
		shadow[5,0] |= 64u;
		shadow[6,0] |= 128u;
	},
//  for an occlusion at [23,8]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 6u;
		shadow[3,0] |= 12u;
		shadow[4,0] |= 24u;
		shadow[5,0] |= 48u;
		shadow[6,0] |= 96u;
		shadow[7,0] |= 192u;
		shadow[8,0] |= 128u;
	},
//  for an occlusion at [23,9]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
		shadow[4,0] |= 6u;
		shadow[5,0] |= 28u;
		shadow[6,0] |= 56u;
		shadow[7,0] |= 112u;
		shadow[8,0] |= 192u;
		shadow[9,0] |= 128u;
	},
//  for an occlusion at [23,10]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2u;
		shadow[6,0] |= 14u;
		shadow[7,0] |= 28u;
		shadow[8,0] |= 120u;
		shadow[9,0] |= 224u;
		shadow[10,0] |= 128u;
	},
//  for an occlusion at [23,11]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2u;
		shadow[8,0] |= 14u;
		shadow[9,0] |= 56u;
		shadow[10,0] |= 224u;
		shadow[11,0] |= 128u;
	},
//  for an occlusion at [23,12]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 14u;
		shadow[10,0] |= 62u;
		shadow[11,0] |= 248u;
		shadow[12,0] |= 192u;
	},
//  for an occlusion at [23,13]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 30u;
		shadow[12,0] |= 254u;
		shadow[13,0] |= 240u;
	},
//  for an occlusion at [23,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 254u;
		shadow[14,0] |= 254u;
	},
//  for an occlusion at [23,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 254u;
	},
//  for an occlusion at [23,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 254u;
		shadow[17,0] |= 254u;
	},
//  for an occlusion at [23,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 240u;
		shadow[18,0] |= 254u;
		shadow[19,0] |= 30u;
	},
//  for an occlusion at [23,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 192u;
		shadow[19,0] |= 248u;
		shadow[20,0] |= 62u;
		shadow[21,0] |= 14u;
	},
//  for an occlusion at [23,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 128u;
		shadow[20,0] |= 224u;
		shadow[21,0] |= 56u;
		shadow[22,0] |= 14u;
		shadow[23,0] |= 2u;
	},
//  for an occlusion at [23,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 128u;
		shadow[21,0] |= 224u;
		shadow[22,0] |= 120u;
		shadow[23,0] |= 28u;
		shadow[24,0] |= 14u;
		shadow[25,0] |= 2u;
	},
//  for an occlusion at [23,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 128u;
		shadow[22,0] |= 192u;
		shadow[23,0] |= 112u;
		shadow[24,0] |= 56u;
		shadow[25,0] |= 28u;
		shadow[26,0] |= 6u;
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [23,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 128u;
		shadow[23,0] |= 192u;
		shadow[24,0] |= 96u;
		shadow[25,0] |= 48u;
		shadow[26,0] |= 24u;
		shadow[27,0] |= 12u;
		shadow[28,0] |= 6u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [23,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 128u;
		shadow[25,0] |= 64u;
		shadow[26,0] |= 32u;
		shadow[27,0] |= 16u;
		shadow[28,0] |= 8u;
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [23,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 384u;
		shadow[26,0] |= 192u;
		shadow[27,0] |= 96u;
		shadow[28,0] |= 48u;
		shadow[29,0] |= 24u;
		shadow[30,0] |= 12u;
	},
//  for an occlusion at [23,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 384u;
		shadow[27,0] |= 192u;
		shadow[28,0] |= 96u;
		shadow[29,0] |= 32u;
		shadow[30,0] |= 16u;
	},
//  for an occlusion at [23,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 384u;
		shadow[28,0] |= 192u;
		shadow[29,0] |= 64u;
		shadow[30,0] |= 32u;
	},
//  for an occlusion at [23,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 384u;
		shadow[29,0] |= 128u;
		shadow[30,0] |= 64u;
	},
//  for an occlusion at [23,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 384u;
		shadow[30,0] |= 128u;
	},
//  for an occlusion at [23,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 128u;
	},
//  for an occlusion at [23,30]:
    null,
  },
  {
//  for an occlusion at [24,0]:
    null,
//  for an occlusion at [24,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 64u;
	},
//  for an occlusion at [24,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 64u;
		shadow[1,0] |= 192u;
	},
//  for an occlusion at [24,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32u;
		shadow[1,0] |= 96u;
		shadow[2,0] |= 192u;
	},
//  for an occlusion at [24,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16u;
		shadow[1,0] |= 48u;
		shadow[2,0] |= 96u;
		shadow[3,0] |= 192u;
	},
//  for an occlusion at [24,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12u;
		shadow[1,0] |= 24u;
		shadow[2,0] |= 48u;
		shadow[3,0] |= 96u;
		shadow[4,0] |= 192u;
	},
//  for an occlusion at [24,6]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
		shadow[2,0] |= 8u;
		shadow[3,0] |= 16u;
		shadow[4,0] |= 32u;
		shadow[5,0] |= 64u;
	},
//  for an occlusion at [24,7]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 6u;
		shadow[3,0] |= 12u;
		shadow[4,0] |= 24u;
		shadow[5,0] |= 48u;
		shadow[6,0] |= 96u;
		shadow[7,0] |= 64u;
	},
//  for an occlusion at [24,8]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
		shadow[4,0] |= 6u;
		shadow[5,0] |= 28u;
		shadow[6,0] |= 48u;
		shadow[7,0] |= 96u;
		shadow[8,0] |= 64u;
	},
//  for an occlusion at [24,9]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 6u;
		shadow[6,0] |= 28u;
		shadow[7,0] |= 56u;
		shadow[8,0] |= 112u;
		shadow[9,0] |= 64u;
	},
//  for an occlusion at [24,10]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2u;
		shadow[7,0] |= 14u;
		shadow[8,0] |= 60u;
		shadow[9,0] |= 112u;
		shadow[10,0] |= 64u;
	},
//  for an occlusion at [24,11]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 30u;
		shadow[10,0] |= 120u;
		shadow[11,0] |= 96u;
	},
//  for an occlusion at [24,12]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 30u;
		shadow[11,0] |= 124u;
		shadow[12,0] |= 112u;
	},
//  for an occlusion at [24,13]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 6u;
		shadow[12,0] |= 126u;
		shadow[13,0] |= 112u;
	},
//  for an occlusion at [24,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 126u;
		shadow[14,0] |= 126u;
	},
//  for an occlusion at [24,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 126u;
	},
//  for an occlusion at [24,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 126u;
		shadow[17,0] |= 126u;
	},
//  for an occlusion at [24,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 112u;
		shadow[18,0] |= 126u;
		shadow[19,0] |= 6u;
	},
//  for an occlusion at [24,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 112u;
		shadow[19,0] |= 124u;
		shadow[20,0] |= 30u;
	},
//  for an occlusion at [24,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 96u;
		shadow[20,0] |= 120u;
		shadow[21,0] |= 30u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [24,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 64u;
		shadow[21,0] |= 112u;
		shadow[22,0] |= 60u;
		shadow[23,0] |= 14u;
		shadow[24,0] |= 2u;
	},
//  for an occlusion at [24,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 64u;
		shadow[22,0] |= 112u;
		shadow[23,0] |= 56u;
		shadow[24,0] |= 28u;
		shadow[25,0] |= 6u;
	},
//  for an occlusion at [24,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 64u;
		shadow[23,0] |= 96u;
		shadow[24,0] |= 48u;
		shadow[25,0] |= 28u;
		shadow[26,0] |= 6u;
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [24,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 64u;
		shadow[24,0] |= 96u;
		shadow[25,0] |= 48u;
		shadow[26,0] |= 24u;
		shadow[27,0] |= 12u;
		shadow[28,0] |= 6u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [24,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 64u;
		shadow[26,0] |= 32u;
		shadow[27,0] |= 16u;
		shadow[28,0] |= 8u;
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [24,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 192u;
		shadow[27,0] |= 96u;
		shadow[28,0] |= 48u;
		shadow[29,0] |= 24u;
		shadow[30,0] |= 12u;
	},
//  for an occlusion at [24,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 192u;
		shadow[28,0] |= 96u;
		shadow[29,0] |= 48u;
		shadow[30,0] |= 16u;
	},
//  for an occlusion at [24,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 192u;
		shadow[29,0] |= 96u;
		shadow[30,0] |= 32u;
	},
//  for an occlusion at [24,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 192u;
		shadow[30,0] |= 64u;
	},
//  for an occlusion at [24,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 64u;
	},
//  for an occlusion at [24,30]:
    null,
  },
  {
//  for an occlusion at [25,0]:
    null,
//  for an occlusion at [25,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 32u;
	},
//  for an occlusion at [25,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 48u;
		shadow[1,0] |= 96u;
	},
//  for an occlusion at [25,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 24u;
		shadow[1,0] |= 48u;
		shadow[2,0] |= 96u;
	},
//  for an occlusion at [25,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 12u;
		shadow[1,0] |= 24u;
		shadow[2,0] |= 48u;
		shadow[3,0] |= 96u;
	},
//  for an occlusion at [25,5]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
		shadow[2,0] |= 8u;
		shadow[3,0] |= 16u;
		shadow[4,0] |= 32u;
	},
//  for an occlusion at [25,6]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 6u;
		shadow[3,0] |= 12u;
		shadow[4,0] |= 24u;
		shadow[5,0] |= 48u;
		shadow[6,0] |= 32u;
	},
//  for an occlusion at [25,7]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
		shadow[4,0] |= 12u;
		shadow[5,0] |= 24u;
		shadow[6,0] |= 48u;
		shadow[7,0] |= 32u;
	},
//  for an occlusion at [25,8]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2u;
		shadow[5,0] |= 14u;
		shadow[6,0] |= 28u;
		shadow[7,0] |= 48u;
		shadow[8,0] |= 32u;
	},
//  for an occlusion at [25,9]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 6u;
		shadow[7,0] |= 28u;
		shadow[8,0] |= 56u;
		shadow[9,0] |= 32u;
	},
//  for an occlusion at [25,10]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2u;
		shadow[8,0] |= 14u;
		shadow[9,0] |= 56u;
		shadow[10,0] |= 32u;
	},
//  for an occlusion at [25,11]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 14u;
		shadow[10,0] |= 60u;
		shadow[11,0] |= 48u;
	},
//  for an occlusion at [25,12]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 6u;
		shadow[11,0] |= 62u;
		shadow[12,0] |= 56u;
	},
//  for an occlusion at [25,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 62u;
		shadow[13,0] |= 56u;
	},
//  for an occlusion at [25,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 62u;
		shadow[14,0] |= 62u;
	},
//  for an occlusion at [25,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 62u;
	},
//  for an occlusion at [25,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 62u;
		shadow[17,0] |= 62u;
	},
//  for an occlusion at [25,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 56u;
		shadow[18,0] |= 62u;
	},
//  for an occlusion at [25,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 56u;
		shadow[19,0] |= 62u;
		shadow[20,0] |= 6u;
	},
//  for an occlusion at [25,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 48u;
		shadow[20,0] |= 60u;
		shadow[21,0] |= 14u;
	},
//  for an occlusion at [25,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 32u;
		shadow[21,0] |= 56u;
		shadow[22,0] |= 14u;
		shadow[23,0] |= 2u;
	},
//  for an occlusion at [25,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 32u;
		shadow[22,0] |= 56u;
		shadow[23,0] |= 28u;
		shadow[24,0] |= 6u;
	},
//  for an occlusion at [25,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 32u;
		shadow[23,0] |= 48u;
		shadow[24,0] |= 28u;
		shadow[25,0] |= 14u;
		shadow[26,0] |= 2u;
	},
//  for an occlusion at [25,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 32u;
		shadow[24,0] |= 48u;
		shadow[25,0] |= 24u;
		shadow[26,0] |= 12u;
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [25,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 32u;
		shadow[25,0] |= 48u;
		shadow[26,0] |= 24u;
		shadow[27,0] |= 12u;
		shadow[28,0] |= 6u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [25,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 32u;
		shadow[27,0] |= 16u;
		shadow[28,0] |= 8u;
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [25,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 96u;
		shadow[28,0] |= 48u;
		shadow[29,0] |= 24u;
		shadow[30,0] |= 12u;
	},
//  for an occlusion at [25,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 96u;
		shadow[29,0] |= 48u;
		shadow[30,0] |= 24u;
	},
//  for an occlusion at [25,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 96u;
		shadow[30,0] |= 48u;
	},
//  for an occlusion at [25,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 32u;
	},
//  for an occlusion at [25,30]:
    null,
  },
  {
//  for an occlusion at [26,0]:
    null,
//  for an occlusion at [26,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 16u;
	},
//  for an occlusion at [26,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8u;
		shadow[1,0] |= 16u;
	},
//  for an occlusion at [26,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4u;
		shadow[1,0] |= 8u;
		shadow[2,0] |= 16u;
	},
//  for an occlusion at [26,4]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
		shadow[2,0] |= 8u;
		shadow[3,0] |= 16u;
	},
//  for an occlusion at [26,5]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 6u;
		shadow[3,0] |= 12u;
		shadow[4,0] |= 24u;
		shadow[5,0] |= 16u;
	},
//  for an occlusion at [26,6]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 6u;
		shadow[4,0] |= 12u;
		shadow[5,0] |= 24u;
		shadow[6,0] |= 16u;
	},
//  for an occlusion at [26,7]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2u;
		shadow[5,0] |= 12u;
		shadow[6,0] |= 24u;
		shadow[7,0] |= 16u;
	},
//  for an occlusion at [26,8]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2u;
		shadow[6,0] |= 14u;
		shadow[7,0] |= 28u;
		shadow[8,0] |= 16u;
	},
//  for an occlusion at [26,9]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 6u;
		shadow[8,0] |= 28u;
		shadow[9,0] |= 16u;
	},
//  for an occlusion at [26,10]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 28u;
		shadow[10,0] |= 16u;
	},
//  for an occlusion at [26,11]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 6u;
		shadow[10,0] |= 30u;
		shadow[11,0] |= 24u;
	},
//  for an occlusion at [26,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 30u;
		shadow[12,0] |= 24u;
	},
//  for an occlusion at [26,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 30u;
		shadow[13,0] |= 28u;
	},
//  for an occlusion at [26,14]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 30u;
		shadow[14,0] |= 30u;
	},
//  for an occlusion at [26,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 30u;
	},
//  for an occlusion at [26,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 30u;
		shadow[17,0] |= 30u;
	},
//  for an occlusion at [26,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 28u;
		shadow[18,0] |= 30u;
	},
//  for an occlusion at [26,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 24u;
		shadow[19,0] |= 30u;
	},
//  for an occlusion at [26,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 24u;
		shadow[20,0] |= 30u;
		shadow[21,0] |= 6u;
	},
//  for an occlusion at [26,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 16u;
		shadow[21,0] |= 28u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [26,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 16u;
		shadow[22,0] |= 28u;
		shadow[23,0] |= 6u;
	},
//  for an occlusion at [26,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 16u;
		shadow[23,0] |= 28u;
		shadow[24,0] |= 14u;
		shadow[25,0] |= 2u;
	},
//  for an occlusion at [26,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 16u;
		shadow[24,0] |= 24u;
		shadow[25,0] |= 12u;
		shadow[26,0] |= 2u;
	},
//  for an occlusion at [26,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 16u;
		shadow[25,0] |= 24u;
		shadow[26,0] |= 12u;
		shadow[27,0] |= 6u;
	},
//  for an occlusion at [26,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 16u;
		shadow[26,0] |= 24u;
		shadow[27,0] |= 12u;
		shadow[28,0] |= 6u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [26,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 16u;
		shadow[28,0] |= 8u;
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [26,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 16u;
		shadow[29,0] |= 8u;
		shadow[30,0] |= 4u;
	},
//  for an occlusion at [26,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 16u;
		shadow[30,0] |= 8u;
	},
//  for an occlusion at [26,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 16u;
	},
//  for an occlusion at [26,30]:
    null,
  },
  {
//  for an occlusion at [27,0]:
    null,
//  for an occlusion at [27,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 8u;
	},
//  for an occlusion at [27,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4u;
		shadow[1,0] |= 8u;
	},
//  for an occlusion at [27,3]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
		shadow[2,0] |= 8u;
	},
//  for an occlusion at [27,4]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 4u;
		shadow[3,0] |= 8u;
	},
//  for an occlusion at [27,5]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
		shadow[3,0] |= 6u;
		shadow[4,0] |= 12u;
		shadow[5,0] |= 8u;
	},
//  for an occlusion at [27,6]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 6u;
		shadow[5,0] |= 12u;
		shadow[6,0] |= 8u;
	},
//  for an occlusion at [27,7]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2u;
		shadow[6,0] |= 12u;
		shadow[7,0] |= 8u;
	},
//  for an occlusion at [27,8]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2u;
		shadow[7,0] |= 12u;
		shadow[8,0] |= 8u;
	},
//  for an occlusion at [27,9]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2u;
		shadow[8,0] |= 14u;
		shadow[9,0] |= 8u;
	},
//  for an occlusion at [27,10]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 14u;
		shadow[10,0] |= 8u;
	},
//  for an occlusion at [27,11]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 14u;
		shadow[11,0] |= 8u;
	},
//  for an occlusion at [27,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 14u;
		shadow[12,0] |= 12u;
	},
//  for an occlusion at [27,13]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 14u;
		shadow[13,0] |= 14u;
	},
//  for an occlusion at [27,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 14u;
	},
//  for an occlusion at [27,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 14u;
	},
//  for an occlusion at [27,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 14u;
	},
//  for an occlusion at [27,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 14u;
		shadow[18,0] |= 14u;
	},
//  for an occlusion at [27,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 12u;
		shadow[19,0] |= 14u;
	},
//  for an occlusion at [27,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 8u;
		shadow[20,0] |= 14u;
	},
//  for an occlusion at [27,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 8u;
		shadow[21,0] |= 14u;
	},
//  for an occlusion at [27,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 8u;
		shadow[22,0] |= 14u;
		shadow[23,0] |= 2u;
	},
//  for an occlusion at [27,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 8u;
		shadow[23,0] |= 12u;
		shadow[24,0] |= 2u;
	},
//  for an occlusion at [27,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 8u;
		shadow[24,0] |= 12u;
		shadow[25,0] |= 2u;
	},
//  for an occlusion at [27,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 8u;
		shadow[25,0] |= 12u;
		shadow[26,0] |= 6u;
	},
//  for an occlusion at [27,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 8u;
		shadow[26,0] |= 12u;
		shadow[27,0] |= 6u;
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [27,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 8u;
		shadow[28,0] |= 4u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [27,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 8u;
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [27,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 8u;
		shadow[30,0] |= 4u;
	},
//  for an occlusion at [27,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 8u;
	},
//  for an occlusion at [27,30]:
    null,
  },
  {
//  for an occlusion at [28,0]:
    null,
//  for an occlusion at [28,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 4u;
	},
//  for an occlusion at [28,2]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
		shadow[1,0] |= 4u;
	},
//  for an occlusion at [28,3]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
		shadow[2,0] |= 4u;
	},
//  for an occlusion at [28,4]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
		shadow[3,0] |= 4u;
	},
//  for an occlusion at [28,5]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
		shadow[4,0] |= 6u;
		shadow[5,0] |= 4u;
	},
//  for an occlusion at [28,6]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 6u;
		shadow[6,0] |= 4u;
	},
//  for an occlusion at [28,7]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 6u;
		shadow[7,0] |= 4u;
	},
//  for an occlusion at [28,8]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 6u;
		shadow[8,0] |= 4u;
	},
//  for an occlusion at [28,9]:
    delegate( uint[,] shadow )
	{
		shadow[8,0] |= 6u;
		shadow[9,0] |= 4u;
	},
//  for an occlusion at [28,10]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 6u;
		shadow[10,0] |= 4u;
	},
//  for an occlusion at [28,11]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 6u;
		shadow[11,0] |= 4u;
	},
//  for an occlusion at [28,12]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 6u;
		shadow[12,0] |= 6u;
	},
//  for an occlusion at [28,13]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 6u;
	},
//  for an occlusion at [28,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 6u;
	},
//  for an occlusion at [28,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 6u;
	},
//  for an occlusion at [28,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 6u;
	},
//  for an occlusion at [28,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 6u;
	},
//  for an occlusion at [28,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 6u;
		shadow[19,0] |= 6u;
	},
//  for an occlusion at [28,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 4u;
		shadow[20,0] |= 6u;
	},
//  for an occlusion at [28,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 4u;
		shadow[21,0] |= 6u;
	},
//  for an occlusion at [28,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 4u;
		shadow[22,0] |= 6u;
	},
//  for an occlusion at [28,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 4u;
		shadow[23,0] |= 6u;
	},
//  for an occlusion at [28,23]:
    delegate( uint[,] shadow )
	{
		shadow[23,0] |= 4u;
		shadow[24,0] |= 6u;
	},
//  for an occlusion at [28,24]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 4u;
		shadow[25,0] |= 6u;
	},
//  for an occlusion at [28,25]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 4u;
		shadow[26,0] |= 6u;
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [28,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 4u;
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [28,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 4u;
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [28,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 4u;
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [28,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 4u;
	},
//  for an occlusion at [28,30]:
    null,
  },
  {
//  for an occlusion at [29,0]:
    null,
//  for an occlusion at [29,1]:
    delegate( uint[,] shadow )
	{
		shadow[0,0] |= 2u;
	},
//  for an occlusion at [29,2]:
    delegate( uint[,] shadow )
	{
		shadow[1,0] |= 2u;
	},
//  for an occlusion at [29,3]:
    delegate( uint[,] shadow )
	{
		shadow[2,0] |= 2u;
	},
//  for an occlusion at [29,4]:
    delegate( uint[,] shadow )
	{
		shadow[3,0] |= 2u;
	},
//  for an occlusion at [29,5]:
    delegate( uint[,] shadow )
	{
		shadow[4,0] |= 2u;
	},
//  for an occlusion at [29,6]:
    delegate( uint[,] shadow )
	{
		shadow[5,0] |= 2u;
	},
//  for an occlusion at [29,7]:
    delegate( uint[,] shadow )
	{
		shadow[6,0] |= 2u;
	},
//  for an occlusion at [29,8]:
    delegate( uint[,] shadow )
	{
		shadow[7,0] |= 2u;
		shadow[8,0] |= 2u;
	},
//  for an occlusion at [29,9]:
    delegate( uint[,] shadow )
	{
		shadow[9,0] |= 2u;
	},
//  for an occlusion at [29,10]:
    delegate( uint[,] shadow )
	{
		shadow[10,0] |= 2u;
	},
//  for an occlusion at [29,11]:
    delegate( uint[,] shadow )
	{
		shadow[11,0] |= 2u;
	},
//  for an occlusion at [29,12]:
    delegate( uint[,] shadow )
	{
		shadow[12,0] |= 2u;
	},
//  for an occlusion at [29,13]:
    delegate( uint[,] shadow )
	{
		shadow[13,0] |= 2u;
	},
//  for an occlusion at [29,14]:
    delegate( uint[,] shadow )
	{
		shadow[14,0] |= 2u;
	},
//  for an occlusion at [29,15]:
    delegate( uint[,] shadow )
	{
		shadow[15,0] |= 2u;
	},
//  for an occlusion at [29,16]:
    delegate( uint[,] shadow )
	{
		shadow[16,0] |= 2u;
	},
//  for an occlusion at [29,17]:
    delegate( uint[,] shadow )
	{
		shadow[17,0] |= 2u;
	},
//  for an occlusion at [29,18]:
    delegate( uint[,] shadow )
	{
		shadow[18,0] |= 2u;
	},
//  for an occlusion at [29,19]:
    delegate( uint[,] shadow )
	{
		shadow[19,0] |= 2u;
	},
//  for an occlusion at [29,20]:
    delegate( uint[,] shadow )
	{
		shadow[20,0] |= 2u;
	},
//  for an occlusion at [29,21]:
    delegate( uint[,] shadow )
	{
		shadow[21,0] |= 2u;
	},
//  for an occlusion at [29,22]:
    delegate( uint[,] shadow )
	{
		shadow[22,0] |= 2u;
		shadow[23,0] |= 2u;
	},
//  for an occlusion at [29,23]:
    delegate( uint[,] shadow )
	{
		shadow[24,0] |= 2u;
	},
//  for an occlusion at [29,24]:
    delegate( uint[,] shadow )
	{
		shadow[25,0] |= 2u;
	},
//  for an occlusion at [29,25]:
    delegate( uint[,] shadow )
	{
		shadow[26,0] |= 2u;
	},
//  for an occlusion at [29,26]:
    delegate( uint[,] shadow )
	{
		shadow[27,0] |= 2u;
	},
//  for an occlusion at [29,27]:
    delegate( uint[,] shadow )
	{
		shadow[28,0] |= 2u;
	},
//  for an occlusion at [29,28]:
    delegate( uint[,] shadow )
	{
		shadow[29,0] |= 2u;
	},
//  for an occlusion at [29,29]:
    delegate( uint[,] shadow )
	{
		shadow[30,0] |= 2u;
	},
//  for an occlusion at [29,30]:
    null,
  },
  {
//  for an occlusion at [30,0]:
    null,
//  for an occlusion at [30,1]:
    null,
//  for an occlusion at [30,2]:
    null,
//  for an occlusion at [30,3]:
    null,
//  for an occlusion at [30,4]:
    null,
//  for an occlusion at [30,5]:
    null,
//  for an occlusion at [30,6]:
    null,
//  for an occlusion at [30,7]:
    null,
//  for an occlusion at [30,8]:
    null,
//  for an occlusion at [30,9]:
    null,
//  for an occlusion at [30,10]:
    null,
//  for an occlusion at [30,11]:
    null,
//  for an occlusion at [30,12]:
    null,
//  for an occlusion at [30,13]:
    null,
//  for an occlusion at [30,14]:
    null,
//  for an occlusion at [30,15]:
    null,
//  for an occlusion at [30,16]:
    null,
//  for an occlusion at [30,17]:
    null,
//  for an occlusion at [30,18]:
    null,
//  for an occlusion at [30,19]:
    null,
//  for an occlusion at [30,20]:
    null,
//  for an occlusion at [30,21]:
    null,
//  for an occlusion at [30,22]:
    null,
//  for an occlusion at [30,23]:
    null,
//  for an occlusion at [30,24]:
    null,
//  for an occlusion at [30,25]:
    null,
//  for an occlusion at [30,26]:
    null,
//  for an occlusion at [30,27]:
    null,
//  for an occlusion at [30,28]:
    null,
//  for an occlusion at [30,29]:
    null,
//  for an occlusion at [30,30]:
    null,
  },
};
		//------------------------------------------------------------------------------
		//  LOS shadow testers for visibility fields 31 cells square.
		//------------------------------------------------------------------------------
		internal static ShadowTester[/* 31 */] m_ShadowTesters =
		{
//  for an object in position at [XXX,0]:
//  X-------------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 2147483648u ) == 2147483648u ) return true; else return false;
	},
//  for an object in position at [XXX,1]:
//  -X------------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 1073741824u ) == 1073741824u ) return true; else return false;
	},
//  for an object in position at [XXX,2]:
//  --X-----------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 536870912u ) == 536870912u ) return true; else return false;
	},
//  for an object in position at [XXX,3]:
//  ---X----------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 268435456u ) == 268435456u ) return true; else return false;
	},
//  for an object in position at [XXX,4]:
//  ----X---------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 134217728u ) == 134217728u ) return true; else return false;
	},
//  for an object in position at [XXX,5]:
//  -----X--------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 67108864u ) == 67108864u ) return true; else return false;
	},
//  for an object in position at [XXX,6]:
//  ------X-------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 33554432u ) == 33554432u ) return true; else return false;
	},
//  for an object in position at [XXX,7]:
//  -------X------------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 16777216u ) == 16777216u ) return true; else return false;
	},
//  for an object in position at [XXX,8]:
//  --------X-----------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 8388608u ) == 8388608u ) return true; else return false;
	},
//  for an object in position at [XXX,9]:
//  ---------X----------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 4194304u ) == 4194304u ) return true; else return false;
	},
//  for an object in position at [XXX,10]:
//  ----------X---------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 2097152u ) == 2097152u ) return true; else return false;
	},
//  for an object in position at [XXX,11]:
//  -----------X--------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 1048576u ) == 1048576u ) return true; else return false;
	},
//  for an object in position at [XXX,12]:
//  ------------X-------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 524288u ) == 524288u ) return true; else return false;
	},
//  for an object in position at [XXX,13]:
//  -------------X------------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 262144u ) == 262144u ) return true; else return false;
	},
//  for an object in position at [XXX,14]:
//  --------------X-----------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 131072u ) == 131072u ) return true; else return false;
	},
//  for an object in position at [XXX,15]:
//  ---------------X----------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 65536u ) == 65536u ) return true; else return false;
	},
//  for an object in position at [XXX,16]:
//  ----------------X---------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 32768u ) == 32768u ) return true; else return false;
	},
//  for an object in position at [XXX,17]:
//  -----------------X--------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 16384u ) == 16384u ) return true; else return false;
	},
//  for an object in position at [XXX,18]:
//  ------------------X-------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 8192u ) == 8192u ) return true; else return false;
	},
//  for an object in position at [XXX,19]:
//  -------------------X------------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 4096u ) == 4096u ) return true; else return false;
	},
//  for an object in position at [XXX,20]:
//  --------------------X-----------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 2048u ) == 2048u ) return true; else return false;
	},
//  for an object in position at [XXX,21]:
//  ---------------------X----------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 1024u ) == 1024u ) return true; else return false;
	},
//  for an object in position at [XXX,22]:
//  ----------------------X---------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 512u ) == 512u ) return true; else return false;
	},
//  for an object in position at [XXX,23]:
//  -----------------------X--------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 256u ) == 256u ) return true; else return false;
	},
//  for an object in position at [XXX,24]:
//  ------------------------X-------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 128u ) == 128u ) return true; else return false;
	},
//  for an object in position at [XXX,25]:
//  -------------------------X------
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 64u ) == 64u ) return true; else return false;
	},
//  for an object in position at [XXX,26]:
//  --------------------------X-----
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 32u ) == 32u ) return true; else return false;
	},
//  for an object in position at [XXX,27]:
//  ---------------------------X----
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 16u ) == 16u ) return true; else return false;
	},
//  for an object in position at [XXX,28]:
//  ----------------------------X---
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 8u ) == 8u ) return true; else return false;
	},
//  for an object in position at [XXX,29]:
//  -----------------------------X--
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 4u ) == 4u ) return true; else return false;
	},
//  for an object in position at [XXX,30]:
//  ------------------------------X-
    delegate ( uint[,] shadow, int y )
	{
		if( ( shadow[y,0] & 2u ) == 2u ) return true; else return false;
	},
};
	}
} // namespace
