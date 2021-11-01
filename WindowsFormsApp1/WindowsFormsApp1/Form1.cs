using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Wumpus_World ww;
        public Form1()
        {
            InitializeComponent();
            ww = new Wumpus_World();
            for (int i = 0; i < 16; i++)
                realWorld.Controls.Add(new PictureBox());
            foreach (PictureBox pb in realWorld.Controls)
            {
                pb.Height = pb.Width = (realWorld.Height -50) / 4;
                pb.BorderStyle = BorderStyle.Fixed3D;
            }
            for (int i = 0; i < 16; i++)    agentWorld.Controls.Add(new PictureBox());
            foreach (PictureBox pb in agentWorld.Controls)
            {
                pb.Height = pb.Width = (realWorld.Height -50) / 4;
                pb.BorderStyle = BorderStyle.Fixed3D;
            }
            label2.Text = ww.Performance.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void draw_Real_World()
        {
            Graphics g;
            for (int i = 0; i < 16; i++)
            {
                g = ((PictureBox)realWorld.Controls[i]).CreateGraphics();
                g.Clear(SystemColors.Control);
                if (i == 0) g.DrawString("Start", new Font("Adobe Gothic Std", 20f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 0);
                if (ww.world[i % 4 + 1, i / 4 + 1].Pit == 1)
                {
                    g.DrawString("PIT", new Font("Adobe Gothic Std", 36f, FontStyle.Bold), new SolidBrush(Color.Blue), 0, 0);
                }
                if (ww.world[i % 4 + 1, i / 4 + 1].Wumpus == 1)
                {
                    g.DrawString("Wumpus", new Font("Adobe Gothic Std", 14f, FontStyle.Bold), new SolidBrush(Color.Red), 0, 0);
                }
                if (ww.world[i % 4 + 1, i / 4 + 1].Gold == 1)
                {
                    g.DrawString("Gold", new Font("Adobe Gothic Std", 28f, FontStyle.Bold), new SolidBrush(Color.Gold), 0, 0);
                }

                if (ww.world[i % 4 + 1, i / 4 + 1].Stench == 1)
                {
                    if (ww.world[i % 4 + 1, i / 4 + 1].Breeze == 1) g.DrawString("S+B", new Font("Adobe Gothic Std", 24f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 50);
                    else g.DrawString("Stench", new Font("Adobe Gothic Std", 14f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 50);
                }
                else if (ww.world[i % 4 + 1, i / 4 + 1].Breeze == 1)    g.DrawString("Breeze", new Font("Adobe Gothic Std", 14f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 50);
            }
        }

        private void draw_Agent_World()
        {
            Graphics g;

            for (int i = 0; i < 16; i++)
            {
                g = ((PictureBox)agentWorld.Controls[i]).CreateGraphics();
                g.Clear(SystemColors.Control);
                if (i == 0) g.DrawString("Start", new Font("Adobe Gothic Std", 15f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 0);
                if (ww.current.x + 4 * ww.current.y - 5 == i) g.DrawString("*", new Font("Adobe Gothic Std", 15f, FontStyle.Bold), new SolidBrush(Color.Red), 75, 0);
                if (ww.agent_world[i % 4 + 1, i / 4 + 1].safe == true) g.DrawString("SAFE", new Font("Adobe Gothic Std", 22f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 30);
                else
                {
                    if (ww.agent_world[i % 4 + 1, i / 4 + 1].Wumpus == 2) g.DrawString("PO WU", new Font("Adobe Gothic Std", 10f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 30);
                    else if(ww.agent_world[i % 4 + 1, i / 4 + 1].Wumpus == 1) g.DrawString("Wumpus", new Font("Adobe Gothic Std", 14f, FontStyle.Bold), new SolidBrush(Color.Red), 0, 0);

                    if (ww.agent_world[i % 4 + 1, i / 4 + 1].Pit == 2) g.DrawString("PO PIT", new Font("Adobe Gothic Std", 10f, FontStyle.Bold), new SolidBrush(Color.Black), 0, 50);
                    else if (ww.agent_world[i % 4 + 1, i / 4 + 1].Pit == 1) g.DrawString("PIT", new Font("Adobe Gothic Std", 36f, FontStyle.Bold), new SolidBrush(Color.Blue), 0, 0);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ww = new Wumpus_World();
            draw_Real_World();
            draw_Agent_World();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ww.explore();
            ww.decision_ree();
            ww.route.Push(ww.current.x + 4 * ww.current.y - 5);
            ww.agent_world[ww.current.x, ww.current.y].visited = true;
            draw_Real_World();
            draw_Agent_World();
            ww.rule();
            label2.Text = ww.Performance.ToString();
        }
    }

    public class Wumpus_World
    {
        public struct Current
        {
            public int x;
            public int y;
        }
        public struct Block
        {
            public int Breeze;
            public int Stench;
            public int Glitter;
            public int Wumpus;
            public int Pit;
            public int Gold;
            public bool safe;
            public List<int> neigh_wumpus; //初步构想为将不确定的周围节点入List，当确定为安全后再退出，最后如果只剩下一个点了就可以确定其为PIT或WUMPUS
            public List<int> neigh_pit;
            public List<int> real_neigh_pit;
            public bool visited;
            public List<String> operation_List;
            public bool isReturnAlready;
        }
        public Block[,] world;
        public Block[,] agent_world;
        public Current current;
        public int agent_discover_pit;
        public Stack<int> route;
        public bool recentReturnAlready;
        public int previous;
        public bool EOG;
        public int Performance;

        public Wumpus_World()
        {
            world = new Block[6, 6];//六行六列的数组，只取用其中的[1,1]到[4,4]
            agent_world = new Block[6, 6];
            agent_discover_pit = 0;
            for (int i=1;i<=4;i++)
            {
                for(int j = 1; j <= 4; j++)
                {
                    world[i,j].Breeze = 0;
                    world[i, j].Stench = 0;
                    world[i, j].Glitter = 0;
                    world[i, j].Wumpus = 0;
                    world[i, j].Pit = 0;
                    world[i, j].Gold = 0;
                    world[i, j].safe = false;
                }
            }

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    agent_world[i, j].Breeze = -1;
                    agent_world[i, j].Stench = -1;
                    agent_world[i, j].Glitter = -1;
                    agent_world[i, j].Wumpus = -1;
                    agent_world[i, j].Pit = -1;
                    agent_world[i, j].Gold = -1;
                    agent_world[i, j].safe = false;
                    agent_world[i, j].neigh_wumpus = new List<int>();
                    agent_world[i, j].neigh_pit = new List<int>();
                    agent_world[i, j].real_neigh_pit = new List<int>();
                    agent_world[i, j].visited = false;
                    agent_world[i, j].operation_List = new List<string>();
                    agent_world[i, j].operation_List.Add("DOWN");
                    agent_world[i, j].operation_List.Add("RIGHT");
                    agent_world[i, j].operation_List.Add("LEFT");
                    agent_world[i, j].operation_List.Add("UP");
                    agent_world[i, j].isReturnAlready = false;

                    if (i == 4)
                    {
                        agent_world[i, j].operation_List.Remove("RIGHT");
                    }
                    if (j == 4)
                    {
                        agent_world[i, j].operation_List.Remove("DOWN");
                    }
                    if (i == 1)
                    {
                        agent_world[i, j].operation_List.Remove("LEFT");
                    }
                    if (j == 1)
                    {
                        agent_world[i, j].operation_List.Remove("UP");
                    }
                }
            }
            agent_world[1,1].isReturnAlready = true;

            current.x = 1;
            current.y = 1;
            route = new Stack<int>();
            recentReturnAlready = false;
            EOG = false;
            Performance = 0;

            List<int> history = new List<int>();//历史生成位置的列表，确保不重复

            Random rnd = new Random();

            //往世界中添加一个怪物
            int Wumpus_Pos = rnd.Next(1, 15);
            history.Add(Wumpus_Pos);
            world[Wumpus_Pos % 4 + 1, Wumpus_Pos / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", Wumpus_Pos);

            //往世界中添加一个金子
            int Gold_Pos = rnd.Next(1, 15);
            while (history.Contains(Gold_Pos))   Gold_Pos = rnd.Next(1, 15);
            history.Add(Gold_Pos);
            world[Gold_Pos % 4 + 1, Gold_Pos / 4 + 1].Gold = 1;
            setSurroundings("GOLD", Gold_Pos);

            //往世界中添加三个陷阱
            for (int i = 0; i < 3; i++)
            {
                int Pit_Pos = rnd.Next(1, 15);
                while (history.Contains(Pit_Pos))   Pit_Pos = rnd.Next(1, 15);
                history.Add(Pit_Pos);
                world[Pit_Pos % 4 + 1, Pit_Pos / 4 + 1].Pit = 1;
                setSurroundings("PIT", Pit_Pos);
            }
            
            //下面为手动构造的测试样本
            /*
            world[9 % 4 + 1, 9 / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", 9);

            world[5 % 4 + 1, 5 / 4 + 1].Gold = 1;
            setSurroundings("GOLD", 5);

            world[6 % 4 + 1, 6 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 6);
            world[7 % 4 + 1, 7 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 7);
            world[10 % 4 + 1, 10 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 10);
            */

            //Exampe2
            /*
            world[3 % 4 + 1, 3 / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", 3);

            world[9 % 4 + 1, 9 / 4 + 1].Gold = 1;
            setSurroundings("GOLD", 9);

            world[5 % 4 + 1, 5 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 5);
            world[12 % 4 + 1, 12 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 12);
            world[14 % 4 + 1, 14 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 14);
            */

            //Example3
            /*
            world[11 % 4 + 1, 11 / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", 11);

            world[1 % 4 + 1, 1 / 4 + 1].Gold = 1;
            setSurroundings("GOLD", 1);

            world[2 % 4 + 1, 2 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 2);
            world[12 % 4 + 1, 12 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 12);
            world[7 % 4 + 1, 7 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 7);
            */
            /*
            //Exampe4
            world[1 % 4 + 1, 1 / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", 1);

            world[11 % 4 + 1, 11 / 4 + 1].Gold = 1;
            setSurroundings("GOLD", 11);

            world[3 % 4 + 1, 3 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 3);
            world[4 % 4 + 1, 4 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 4);
            world[6 % 4 + 1, 6 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 6);
            */

            //Example 5
            /*
            world[4 % 4 + 1, 4 / 4 + 1].Wumpus = 1;
            setSurroundings("WUMPUS", 4);

            world[11 % 4 + 1, 11 / 4 + 1].Gold = 1;
            setSurroundings("GOLD", 11);

            world[3 % 4 + 1, 3 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 3);
            world[9 % 4 + 1, 9 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 9);
            world[13 % 4 + 1, 13 / 4 + 1].Pit = 1;
            setSurroundings("PIT", 13);
            */
        }
        public void rule()
        {
            if (world[current.x, current.y].Wumpus == 1 || world[current.x, current.y].Pit == 1) { MessageBox.Show("Game Over!"); Performance -= 1000; }
            else if (world[current.x, current.y].Gold == 1) { MessageBox.Show("You Win!"); Performance += 1000; EOG = true; }
        }
        
        public void explore()//此处为根据当前所在节点感知四周情况，具体选择走哪条路请看decision
        {
            if (agent_world[1, 1].safe == false)
            { 
                agent_world[1, 1].safe = true;
                route.Push(0);
            }

            //利用现实世界中当前节点的信息，对agentWorld的地图进行设置
            if (world[current.x, current.y].Breeze != 1 && world[current.x, current.y].Stench != 1)//无风无臭，四周皆Safe
            {
                //int pos = current.x - 1 + (current.y - 1) * 4;
                setAgentSurroundings("SAFE", current.x, current.y);
            }

            if (world[current.x,current.y].Breeze == 1)//有风，四周至少一个PIT，至多三个PIT，此处将四周所有节点设置为可能存在PIT(2)
            {
                
                if (agent_discover_pit == 3)    setAgentSurroundings("NOMOREPIT", current.x, current.y);
                else setAgentSurroundings("POSSIBLE_PIT", current.x, current.y);
            }

            if (world[current.x, current.y].Stench == 1)//有臭，四周一个Wumpus，此处将四周所有节点设置为可能存在WUMPUS(2)
            {
                setAgentSurroundings("POSSIBLE_WUMPUS", current.x, current.y);
            }



            for(int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    if (agent_world[i, j].safe == true) poToNo(i,j);
                    if (agent_world[i, j].neigh_wumpus.Count == 1)
                    {
                        int wu = agent_world[i, j].neigh_wumpus[0];
                        agent_world[wu % 4 + 1, wu / 4 + 1].Wumpus = 1;
                    }
                    if (agent_world[i, j].neigh_pit.Count == 1 && agent_world[i, j].real_neigh_pit.Count == 0)
                    {
                        int pit = agent_world[i, j].neigh_pit[0];
                        agent_world[pit % 4 + 1, pit / 4 + 1].Pit = 1;
                        agent_world[pit % 4 + 1, pit / 4 + 1].Wumpus = 0;
                        agent_discover_pit++;
                        setAgentSurroundings("REALPIT", pit % 4 + 1, pit / 4 + 1);
                    }
                }
            }
        }

        public void decision_ree()
        {
            if (EOG)
            {
                Performance -= returnHome();
            }
            else
            {
                if (current.y + 1 <= 4 && previous / 4 + 1 != current.y + 1 && agent_world[current.x, current.y + 1].safe == true && agent_world[current.x, current.y].operation_List.Contains("DOWN")) { Down();Performance--; }
                else if (current.x + 1 <= 4 && previous % 4 + 1 != current.x + 1 && agent_world[current.x + 1, current.y].safe == true && agent_world[current.x, current.y].operation_List.Contains("RIGHT")) { Right(); Performance--; }
                else if (current.x - 1 >= 1 && previous % 4 + 1 != current.x - 1 && agent_world[current.x - 1, current.y].safe == true && agent_world[current.x, current.y].operation_List.Contains("LEFT")) { Left(); Performance--; }
                else if (current.y - 1 >= 1 && previous / 4 + 1 != current.y - 1 && agent_world[current.x, current.y - 1].safe == true && agent_world[current.x, current.y].operation_List.Contains("UP")) { Up(); Performance--; }
                else if (agent_world[current.x, current.y].isReturnAlready == false) { agent_Back(); }
                else if (agent_world[current.x, current.y].isReturnAlready == true)
                {
                    if (agent_world[current.x, current.y].neigh_wumpus.Count == 2)
                    {
                        int originalX = current.x;
                        int originalY = current.y;
                        bool re = shoot();
                        if (re)
                        {
                            foreach (int i in agent_world[originalX, originalY].neigh_wumpus)
                            {
                                agent_world[i % 4 + 1, i / 4 + 1].Wumpus = 0;
                            }
                            agent_world[originalX, originalY].neigh_wumpus.Clear();
                        }
                    }
                    else if (world[current.x, current.y].Stench == 1 && world[current.x, current.y].Breeze == 1)
                    {
                        int originalX = current.x;
                        int originalY = current.y;
                        bool re = shoot();
                        
                        if (re)
                        {
                            foreach (int i in agent_world[originalX, originalY].neigh_wumpus)
                            {
                                agent_world[i % 4 + 1, i / 4 + 1].Wumpus = 0;
                            }
                            agent_world[originalX, originalY].neigh_wumpus.Clear();
                        }
                        else random_action();//射中了往该方向走，否则轮盘赌
                    }
                    else
                        random_action();
                }
            }
        }

        int returnHome()
        {
            int step = 0;
            while(!(current.x == 1 && current.y == 1))
            {
                if (current.x - 1 >= 1 && agent_world[current.x - 1, current.y].safe == true) current.x--;
                else if (current.y - 1 >= 1 && agent_world[current.x, current.y].safe == true) current.y--;
                else
                {
                    if (current.x + 1 <= 4) current.x++;
                    if (current.y - 1 >= 1) current.y--;
                }
                step++;
            }
            return step;
        }

        bool Down()
        {
            if (++current.y <= 4) 
            { 
                agent_world[current.x, current.y - 1].operation_List.Remove("DOWN"); previous = current.x + 4 * (current.y - 1) - 5; return true; 
            }
            else return false;
        }

        bool Right()
        {
            if (++current.x <= 4)
            {
                agent_world[current.x - 1, current.y].operation_List.Remove("RIGHT"); previous = current.x-1 + 4 * (current.y - 0) - 5; return true;
            }
            else return false;
        }

        bool Left()
        {
            if (--current.x >= 1)
            {
                agent_world[current.x + 1, current.y].operation_List.Remove("LEFT"); previous = current.x + 1 + 4 * (current.y - 0) - 5; return true;
            }
            else return false;
        }

        bool Up()
        {
            if (--current.y >= 1)
            {
                agent_world[current.x, current.y + 1].operation_List.Remove("UP"); previous = current.x - 0 + 4 * (current.y + 1) - 5; return true;
            }
            else return false;
        }
        bool agent_Back()
        {
            Current tmp;
            tmp.x = current.x;
            tmp.y = current.y;

            int i = previous;
            current.y = i / 4 + 1;
            current.x = i % 4 + 1;
            if(current.y >4 || current.y<1 || current.x>4 || current.y < 1) { current.x = tmp.x;current.y = tmp.y; return false; }
            Performance--;
            route.Pop();
            agent_world[tmp.x, tmp.y].isReturnAlready = true;
            return true;
        }
        bool shoot()
        {
            Random rnd = new Random();
            double pos = rnd.NextDouble();
            bool success_gen = false;
            bool success_shoot = false;
            do {
                pos = rnd.NextDouble();
                if (pos >= 0 && pos <= 0.25)
                {
                    if (current.x - 1 < 1) success_gen = false;
                    else
                    {
                        current.x--;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1)
                        {
                            success_gen = false;
                            current.x++;
                        }
                        else
                        {
                            success_gen = true;
                            if (world[current.x, current.y].Wumpus == 1)
                            {
                                success_shoot = true;
                                setSurroundings("REMOVE_WUMPUS", current.x + 4 * current.y - 5);
                                setAgentSurroundings("KILLWUMPUS", current.x, current.y);
                                agent_world[current.x, current.y].Pit = 0;
                            }
                            else
                            {
                                current.x++;
                                success_shoot = false;
                            }
                        }
                    }

                }
                else if (pos > 0.25 && pos <= 0.5)
                {
                    if (current.x + 1 > 4) success_gen = false;
                    else
                    {
                        current.x++;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1)
                        {
                            success_gen = false;
                            current.x--;
                        }
                        else
                        {
                            success_gen = true;
                            if (world[current.x, current.y].Wumpus == 1)
                            {
                                success_shoot = true;
                                setSurroundings("REMOVE_WUMPUS", current.x + 4 * current.y - 5);
                                setAgentSurroundings("KILLWUMPUS", current.x, current.y);
                                agent_world[current.x, current.y].Pit = 0;
                            }
                            else
                            {
                                current.x--;
                                success_shoot = false;
                            }
                        }
                    }

                }
                else if (pos > 0.5 && pos <= 0.75)
                {
                    if (current.y + 1 > 4) success_gen = false;
                    else
                    {
                        current.y++;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1)
                        {
                            success_gen = false;
                            current.y--;
                        }
                        else
                        {
                            success_gen = true;
                            if (world[current.x, current.y].Wumpus == 1)
                            {
                                success_shoot = true;
                                setSurroundings("REMOVE_WUMPUS", current.x + 4 * current.y - 5);
                                setAgentSurroundings("KILLWUMPUS", current.x, current.y);
                                agent_world[current.x, current.y].Pit = 0;
                            }
                            else
                            {
                                current.y--;
                                success_shoot = false;
                            }
                        }
                    }

                }
                else
                {
                    if (current.y - 1 < 1) success_gen = false;
                    else
                    {
                        current.y--;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1)
                        {
                            success_gen = false;
                            current.y++;
                        }
                        else
                        {
                            success_gen = true;
                            if (world[current.x, current.y].Wumpus == 1)
                            {
                                success_shoot = true;
                                setSurroundings("REMOVE_WUMPUS", current.x + 4 * current.y - 5);
                                setAgentSurroundings("KILLWUMPUS", current.x, current.y);
                                agent_world[current.x, current.y].Pit = 0;
                            }
                            else
                            {
                                current.y++;
                                success_shoot = false;
                            }
                        }
                    }
                }
            } while (success_gen == false);
            Performance -= 10;
            return success_shoot;
        }

        void random_action()
        {
            //轮盘赌的思路是四个方向各0.25概率，如果发现随机点是route中的前序节点则重新随机（还要判断边界）
            Random rnd = new Random();
            double pos = rnd.NextDouble();
            bool success = false;
            int originalX = current.x;
            int originalY = current.y;
            int count = 0;
            do
            {
                count++;
                pos = rnd.NextDouble();
                if (pos >= 0 && pos <= 0.25)
                {
                    if (current.x - 1 < 1) success = false;
                    else
                    {
                        current.x--;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1 || agent_world[current.x, current.y].Wumpus == 1 || agent_world[current.x, current.y].Pit == 1)
                        {
                            success = false;
                            current.x++;
                        }
                        else success = true;
                    }
                }
                else if (pos > 0.25 && pos <= 0.5)
                {
                    if (current.x + 1 > 4) success = false;
                    else
                    {
                        current.x++;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1 || agent_world[current.x, current.y].Wumpus == 1 || agent_world[current.x, current.y].Pit == 1)
                        {
                            success = false;
                            current.x--;
                        }
                        else success = true;
                    }
                }
                else if (pos > 0.5 && pos <= 0.75)
                {
                    if (current.y + 1 > 4) success = false;
                    else
                    {
                        current.y++;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1 || agent_world[current.x, current.y].Wumpus == 1 || agent_world[current.x, current.y].Pit == 1)
                        {
                            success = false;
                            current.y--;
                        }
                        else success = true;
                    }
                }
                else
                {
                    if (current.y - 1 < 1) success = false;
                    else
                    {
                        current.y--;
                        int i = previous;
                        if (current.y == i / 4 + 1 && current.x == i % 4 + 1 || agent_world[current.x, current.y].Wumpus == 1 || agent_world[current.x, current.y].Pit == 1)
                        {
                            success = false;
                            current.y++;
                        }
                        else success = true;
                    }
                }
            } while (!success && count <= 100);//防止陷入一个死循环，设置计数器（从2，1回到1，1但是1，2是陷阱）
            if (!success)
            {
                current.x = previous % 4 + 1;
                current.y = previous / 4 + 1;
            }
            if (world[current.x, current.y].Wumpus == 0 && world[current.x, current.y].Pit == 0)//如果轮盘赌选择的方向经过实践没有怪兽和陷阱，则设置为安全并做标识
            {
                agent_world[current.x, current.y].safe = true;
                agent_world[current.x, current.y].Wumpus = 0;
                agent_world[current.x, current.y].Pit = 0;
                agent_world[originalX, originalY].neigh_wumpus.Remove(current.x + 4 * current.y - 5);
                agent_world[originalX, originalY].neigh_pit.Remove(current.x + 4 * current.y - 5);
                if (agent_world[originalX, originalY].neigh_wumpus.Count == 1)
                {
                    int wu = agent_world[originalX, originalY].neigh_wumpus[0];
                    agent_world[wu % 4 + 1, wu / 4 + 1].Wumpus = 1;
                }
                if (agent_world[originalX, originalY].neigh_pit.Count == 1 && agent_world[originalX,originalY].real_neigh_pit.Count == 0)
                {
                    int pit = agent_world[originalX, originalY].neigh_pit[0];
                    agent_world[pit % 4 + 1, pit / 4 + 1].Pit = 1;
                    agent_world[pit % 4 + 1, pit / 4 + 1].Wumpus = 0;
                    agent_discover_pit++;
                    setAgentSurroundings("REALPIT", pit % 4 + 1, pit / 4 + 1);
                }
            }
        }

        void setSurroundings(string str,int pos)
        {
            switch (str)
            {
                case "WUMPUS":
                    world[pos % 4 + 2, pos / 4 + 1].Stench = 1;
                    world[pos % 4 - 0, pos / 4 + 1].Stench = 1;
                    world[pos % 4 + 1, pos / 4 + 2].Stench = 1;
                    world[pos % 4 + 1, pos / 4 - 0].Stench = 1;
                    break;
                case "PIT":
                    world[pos % 4 + 2, pos / 4 + 1].Breeze = 1;
                    world[pos % 4 - 0, pos / 4 + 1].Breeze = 1;
                    world[pos % 4 + 1, pos / 4 + 2].Breeze = 1;
                    world[pos % 4 + 1, pos / 4 - 0].Breeze = 1;
                    break;
                case "GOLD":
                    world[pos % 4 + 1, pos / 4 + 1].Glitter = 1;
                    break;
                case "REMOVE_WUMPUS":
                    world[pos % 4 + 1, pos / 4 + 1].Wumpus = 0;
                    world[pos % 4 + 2, pos / 4 + 1].Stench = 0;
                    world[pos % 4 - 0, pos / 4 + 1].Stench = 0;
                    world[pos % 4 + 1, pos / 4 + 2].Stench = 0;
                    world[pos % 4 + 1, pos / 4 - 0].Stench = 0;
                    break;
            }
        }

        void poToNo(int x,int y)
        {
            if (!(x + 1 > 4)) { agent_world[x + 1, y].neigh_pit.Remove(x + 4 * y - 5); }
            if (!(x - 1 < 1)) { agent_world[x - 1, y].neigh_pit.Remove(x + 4 * y - 5); }
            if (!(y - 1 < 1)) { agent_world[x, y - 1].neigh_pit.Remove(x + 4 * y - 5); }
            if (!(y + 1 > 4)) { agent_world[x, y + 1].neigh_pit.Remove(x + 4 * y - 5); }
        }

        void setAgentSurroundings(string str, int x, int y)
        {
            //坐标转换关系
            //i = x + 4y - 5
            //x = i % 4 + 1
            //y = i / 4 + 1
            switch (str)
            {
                case "SAFE":
                    if (!(x + 1 > 4)) { agent_world[x + 1, y].safe = true; agent_world[x + 1, y].Wumpus = 0; agent_world[x + 1, y].Pit = 0; }
                    if (!(x - 1 < 1)) { agent_world[x - 1, y].safe = true; agent_world[x - 1, y].Wumpus = 0; agent_world[x - 1, y].Pit = 0; }
                    if (!(y - 1 < 1)) { agent_world[x, y - 1].safe = true; agent_world[x, y - 1].Wumpus = 0; agent_world[x, y - 1].Pit = 0; }
                    if (!(y + 1 > 4)) { agent_world[x, y + 1].safe = true; agent_world[x, y + 1].Wumpus = 0; agent_world[x, y + 1].Pit = 0; }

                    break;
                case "POSSIBLE_PIT":// 0为无、1为有、-1为未探索、2为不确定
                    if ((agent_world[x + 1, y].Pit == -1 || agent_world[x + 1, y].Pit == 2) && agent_world[x + 1, y].safe != true) 
                    { agent_world[x + 1, y].Pit = 2; if ((!agent_world[x, y].neigh_pit.Contains(x + 1 + 4 * y - 5)) && x + 1 <= 4) agent_world[x, y].neigh_pit.Add(x + 1 + 4 * y - 5); }

                    if ((agent_world[x - 1, y].Pit == -1 || agent_world[x - 1, y].Pit == 2)  && agent_world[x - 1, y].safe != true) 
                    { agent_world[x - 1, y].Pit = 2; if ((!agent_world[x, y].neigh_pit.Contains(x - 1 + 4 * y - 5)) && x - 1 >= 1) agent_world[x, y].neigh_pit.Add(x - 1 + 4 * y - 5); }

                    if ((agent_world[x, y - 1].Pit == -1 || agent_world[x, y - 1].Pit == 2) && agent_world[x, y - 1].safe != true) 
                    { agent_world[x, y - 1].Pit = 2; if ((!agent_world[x, y].neigh_pit.Contains(x + 4 * (y - 1) - 5)) && y - 1 >= 1) agent_world[x, y].neigh_pit.Add(x + 4 * (y - 1) - 5); }

                    if ((agent_world[x, y + 1].Pit == -1 || agent_world[x, y + 1].Pit == 2) && agent_world[x, y + 1].safe != true) 
                    { agent_world[x, y + 1].Pit = 2; if ((!agent_world[x, y].neigh_pit.Contains(x + 4 * (y + 1) - 5)) && y + 1 <= 4) agent_world[x, y].neigh_pit.Add(x + 4 * (y + 1) - 5); }

                    break;
                case "POSSIBLE_WUMPUS":
                    if ((agent_world[x + 1, y].Wumpus == -1 || agent_world[x + 1, y].Wumpus == 2) && agent_world[x + 1, y].safe != true) 
                    { agent_world[x + 1, y].Wumpus = 2; if ((!agent_world[x, y].neigh_wumpus.Contains(x + 1 + 4 * y - 5)) && x + 1 <= 4) agent_world[x, y].neigh_wumpus.Add(x + 1 + 4 * y - 5); }

                    if ((agent_world[x - 1, y].Wumpus == -1 || agent_world[x - 1, y].Wumpus == 2) && agent_world[x - 1, y].safe != true) 
                    { agent_world[x - 1, y].Wumpus = 2; if ((!agent_world[x, y].neigh_wumpus.Contains(x - 1 + 4 * y - 5)) && x - 1 >= 1) agent_world[x, y].neigh_wumpus.Add(x - 1 + 4 * y - 5); }

                    if ((agent_world[x, y - 1].Wumpus == -1 || agent_world[x, y - 1].Wumpus == 2) && agent_world[x, y - 1].safe != true) 
                    { agent_world[x, y - 1].Wumpus = 2; if ((!agent_world[x, y].neigh_wumpus.Contains(x + 4 * (y - 1) - 5)) && y - 1 >= 1) agent_world[x, y].neigh_wumpus.Add(x + 4 * (y - 1) - 5); }

                    if ((agent_world[x, y + 1].Wumpus == -1 || agent_world[x, y + 1].Wumpus == 2) && agent_world[x, y + 1].safe != true) 
                    { agent_world[x, y + 1].Wumpus = 2; if ((!agent_world[x, y].neigh_wumpus.Contains(x + 4 * (y + 1) - 5)) && y + 1 <= 4) agent_world[x, y].neigh_wumpus.Add(x + 4 * (y + 1) - 5); }

                    break;

                case "REALPIT":
                    if (x - 1 >= 1) { agent_world[x - 1, y].real_neigh_pit.Add(x - 1 + 4 * y - 5); }
                    if (x + 1 <= 4) { agent_world[x + 1, y].real_neigh_pit.Add(x + 1 + 4 * y - 5); }
                    if (y - 1 >= 1) { agent_world[x, y - 1].real_neigh_pit.Add(x + 4 * (y - 1) - 5); }
                    if (y + 1 <= 4) { agent_world[x, y + 1].real_neigh_pit.Add(x + 4 * (y + 1) - 5); }
                    break;
                case "KILLWUMPUS":
                    agent_world[x, y].Wumpus = 0;
                    agent_world[x, y].Pit = 0;
                    agent_world[x, y].safe = true;
                    break;
                case "NOMOREPIT":
                    if ((agent_world[x + 1, y].Pit == -1 || agent_world[x + 1, y].Pit == 2) && agent_world[x + 1, y].safe != true) { agent_world[x + 1, y].Pit = 0; }
                    if ((agent_world[x - 1, y].Pit == -1 || agent_world[x - 1, y].Pit == 2) && agent_world[x - 1, y].safe != true) { agent_world[x - 1, y].Pit = 0; }
                    if ((agent_world[x, y + 1].Pit == -1 || agent_world[x, y + 1].Pit == 2) && agent_world[x, y + 1].safe != true) { agent_world[x, y + 1].Pit = 0; }
                    if ((agent_world[x, y - 1].Pit == -1 || agent_world[x, y - 1].Pit == 2) && agent_world[x, y - 1].safe != true) { agent_world[x, y - 1].Pit = 0; }
                    break;
            }
        }   
    }
}
