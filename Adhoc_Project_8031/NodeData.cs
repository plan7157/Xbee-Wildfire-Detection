using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adhoc_Project_8031
{
    public class NodeData
    {

        public DataTable Nodetable= new DataTable();

        public NodeData()
        {

            Nodetable.Columns.Add("NodeNames", typeof(string));
            Nodetable.Columns.Add("NodeAddresses", typeof(string));
            Nodetable.Columns.Add("NodeStatus", typeof(int));
            Nodetable.Columns.Add("NodeXaxis", typeof(int));
            Nodetable.Columns.Add("NodeYaxis", typeof(int));

        }
       public void pushNodeData(string NodeName,string NodeAadress)
        {
            Nodetable.Rows.Add(NodeName, NodeAadress,0,-1,-1);
        }
        public DataTable getNodeTable()
        {
            return Nodetable;
        }
        public void pushNodeAxis(int index,int X, int Y)
        {
            Nodetable.Rows[index][3] = X;
            Nodetable.Rows[index][4] = Y;
        }
        public void statusUpdate(int index, int X)
        {
              Nodetable.Rows[index][2] = X;
        }
        public string getAddress(int index)
        {
            return Nodetable.Rows[index][1].ToString();
        }
        public void removeRow(int index)
        {
            Nodetable.Rows[index].Delete();
        }
        public void updateNode(string name, string address,int index)
        {
            Nodetable.Rows[index][0] = name;
            Nodetable.Rows[index][1] = address;
        }
        
    }
}
