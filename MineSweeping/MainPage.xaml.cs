﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MineSweeping
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 

    class MineTag
    {
        private int _PosX;
        public int PosX
        {
            get
            {
                return _PosX;
            }
            set
            {
                _PosX = value;
            }
        }
        
        private int _PosY;
        public int PosY
        {
            get
            {
                return _PosY;
            }
            set
            {
                _PosY = value;
            }
        }

        private bool _IsMine;
        public bool IsMine
        {
            get
            {
                return _IsMine;
            }
            set
            {
                _IsMine = value;
            }
        }

        public MineTag(int x, int y)
        {
            PosX = x;
            PosY = y;
            IsMine = false;
        }

    }
    public sealed partial class MainPage : Page
    {
        private const  int DefaultMineNum = 10;
        private const int DefaultRow = 9;
        private const int DefaultColumn = 9;

        private int MineNum = DefaultMineNum;
        private int Row = DefaultRow;
        private int Column = DefaultColumn;

        private int totalSquares;
        private Button[,] mineControl;

        private bool firstClick = true;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeMineForm();

        }

        private void InitializeMineForm(
            int row = DefaultRow, 
            int column = DefaultColumn)
        {
            totalSquares = row * column;
            mineControl = new Button[row, column];

            Random rand = new Random(Guid.NewGuid().GetHashCode());


            for (int i = 0; i < row; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(74);
                mineArea.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < row; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(74);
                mineArea.ColumnDefinitions.Add(cd);
            }

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    mineControl[i, j] = new Button
                    {
                        Tag = new MineTag(i, j),
                        Width = 50, 
                        Height = 50,
                        Background = new SolidColorBrush(Colors.Red),
                        Margin = new Thickness(10, 10, 10, 10)
                    };

                    mineControl[i, j].Click += new RoutedEventHandler(MineControlClick);
                    mineControl[i, j].RightTapped 
                        += new RightTappedEventHandler(MineControlRightClick);

                    mineArea.Children.Add(mineControl[i, j]);
                    Grid.SetRow(mineControl[i, j], i);
                    Grid.SetColumn(mineControl[i, j], j);
                }
            }
        }

        private void MineControlClick(object sender, RoutedEventArgs e)
        {
            Button mine = sender as Button;
            mine.IsEnabled = false;
            MineTag mineTag = mine.Tag as MineTag;

            if(firstClick)
            {
                InitializeMine(mineTag.PosX, mineTag.PosY);
                firstClick = false;
            }

            if(mineTag.IsMine)
            {
                mine.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                int mineNum = GetNeighborMineNum(mineTag.PosX, mineTag.PosY);
                //mine.Background = new SolidColorBrush(Colors.Green);
                mine.Content = mineNum.ToString();
                if (mineNum == 0)
                {
                    CheckNeighborMine(mineTag.PosX, mineTag.PosY);
                }
            }
        }

        private void MineControlRightClick(object sender, RightTappedRoutedEventArgs e)
        {
            Button mine = sender as Button;
            SolidColorBrush scb = mine.Background as SolidColorBrush;
            if(scb.Color != Colors.Blue)
            {
                mine.Background = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                mine.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void InitializeMine(
            int x, int y,
            int mineNum = DefaultMineNum,
            int row = DefaultRow, 
            int column = DefaultColumn)
        {
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            for(int i = 0; i < mineNum; )
            {
                int minePos = rd.Next(0, totalSquares);
                int minePosX = minePos / row;
                int minePosY = minePos % column;

                MineTag mt = mineControl[minePosX, minePosY].Tag as MineTag;
                if(mt.IsMine || (x == minePosX && y == minePosY))
                {
                    continue;
                }
                else
                {
                    mt.IsMine = true;
                    i++;
                }
            }

        }

        private int GetNeighborMineNum(int posX, int posY)
        {
            int totalNum = 0;
            int minX = posX - 1 > 0 ? posX - 1 : 0;
            int maxX = posX + 1 < Row ? posX + 1 : Row - 1;
            int minY = posY - 1 > 0 ? posY - 1 : 0;
            int maxY = posY + 1 < Column ? posY + 1 : Column - 1;

            for (int i = minX; i <= maxX; i++ )
            {
                for(int j = minY; j <= maxY; j++)
                {
                    MineTag mt = mineControl[i, j].Tag as MineTag;
                    if(mt.IsMine)
                    {
                        totalNum++;
                    }
                }
            }
                return totalNum;
        }
        
        private void CheckNeighborMine(int posX, int posY)
        {
            bool[,] checkStatus = new bool[Row, Column];

            for (int i = 0; i < Row; i++ )
            {
                for (int j = 0; j < Column; j++ )
                {
                    checkStatus[i, j] = false;
                }
            }

            Stack<Button> mineStack = new Stack<Button>();

            mineStack.Push(mineControl[posX, posY]);

            while (mineStack.Count != 0)
            {
                Button mine = mineStack.Pop();
                MineTag mt = mine.Tag as MineTag;
                if (checkStatus[mt.PosX, mt.PosY] != true)
                {
                    checkStatus[mt.PosX, mt.PosY] = true;
                    int mineNum = GetNeighborMineNum(mt.PosX, mt.PosY);
                    if (mineNum == 0)
                    {
                        mine.IsEnabled = false;
                        mine.Content = mineNum;
                        int minX = mt.PosX - 1 > 0 ? mt.PosX - 1 : 0;
                        int maxX = mt.PosX + 1 < Row ? mt.PosX + 1 : Row - 1;
                        int minY = mt.PosY - 1 > 0 ? mt.PosY - 1 : 0;
                        int maxY = mt.PosY + 1 < Column ? mt.PosY + 1 : Column - 1;

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                mineStack.Push(mineControl[i, j]);
                            }
                        }
                    }
                    else
                    {
                        MineControlClick(mine, null);
                    }
                }
            }

        }
    }
}
