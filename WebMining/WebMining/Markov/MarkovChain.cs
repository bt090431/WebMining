﻿using System.Collections.Generic;

namespace WebMining
{
    public class MarkovChain<T>
    {
        IDictionary<T, MarkovNode<T>> items;

        MarkovNode<T> startnode;
        MarkovNode<T> endnode;

        public MarkovChain()
        {
            items = new Dictionary<T, MarkovNode<T>>();

            startnode = new MarkovNode<T>() { State = MarkovState.Start};
            endnode = new MarkovNode<T>() { State = MarkovState.End };
        }

        public MarkovChain<T> setStartNodeName(T t)
        {
            startnode.Value = t;
            return this;
        }
        public MarkovNode<T> getStartNode()
        {
            return startnode;
        }

        public MarkovChain<T> setEndNodeName(T t)
        {
            endnode.Value = t;
            return this;
        }

        public MarkovChain<T> AddTransaction(IList<T> list)
        {
            list.GetEnumerator().MoveNext();
            AddTransaction(list[0], list);
            return this;
        }
        public MarkovChain<T> AddTransaction(T f, IEnumerable<T> list)
        {
            connectToEnd(addAll(connectToStart(getItemAndAddIfNotFound(f)), list));
            return this;
        }
        public MarkovChain<T> AddTransaction(T f, params T[] list)
        {
            connectToEnd(addAll(connectToStart(getItemAndAddIfNotFound(f)), list));
            return this;
        }


        public IEnumerable<T> PredicteNextValues(T t)
        {
            return getItemAndAddIfNotFound(t).GetNextsValues();
        }

        public IEnumerable<MarkovNode<T>> PredicteNext(T t)
        {
            return getItemAndAddIfNotFound(t).GetNexts();
        }
        public IEnumerable<KeyValuePair<MarkovNode<T>,double>> PredicteNextWithProbabilities(T t)
        {
            return getItemAndAddIfNotFound(t).GetNextsWithProbabilities();
        }


        public double Probability(T f,T t)
        {
            var x = new List<MarkovNode<T>>();
            x.Add(getItemAndAddIfNotFound(f));
            return searchForNode(getItemAndAddIfNotFound(f), getItemAndAddIfNotFound(t), x);
        }

        private double searchForNode(MarkovNode<T> start, MarkovNode<T> end, List<MarkovNode<T>> visited)
        {
            //
            // this method gave incorrect result
            // check it later
            //

            double pro = 0;
            foreach (var f in start.GetNexts())
            {
                if (visited.Contains(f))
                    continue;

                visited.Add(f);

                if (f == end)
                {
                    pro = 1;
                    for (int i = 0; i < visited.Count - 1; i++)
                        pro *= visited[i].ProbabilityOf(visited[i + 1]);
                    return pro;
                }
                else
                {
                    pro += searchForNode(f, end, new List<MarkovNode<T>>(visited));
                }
                visited.Remove(f);
            }
            return pro;
        }

        public MarkovNode<T> GetItem(T t)
        {
            return getItemAndAddIfNotFound(t);
        }
        public IEnumerable<T> GetAllItems()
        {
            return items.Keys;
        }


        private MarkovNode<T> addAll(MarkovNode<T> mover, IEnumerable<T> list)
        {
            foreach (var c in list)
            {
                mover.AddNext(getItemAndAddIfNotFound(c));
                mover = getItemAndAddIfNotFound(c);
            }
            return mover;
        }

        private MarkovNode<T> connectToStart(MarkovNode<T> mover)
        {
            startnode.AddNext(mover);
            return mover;
        }

        private MarkovNode<T> connectToEnd(MarkovNode<T> mover)
        {
            mover.AddNext(endnode);
            return mover ;
        }

        private MarkovNode<T> getItemAndAddIfNotFound(T t)
        {
            if (items.ContainsKey(t) == false)
                items.Add(t, new MarkovNode<T>(t));
            return items[t];
        }






        public static string Test()
        {
            var r = new MarkovChain<string>();
            r.setEndNodeName("END");
            r.setStartNodeName("START");
            r.AddTransaction("A", "B");
            r.AddTransaction("A", "B");
            r.AddTransaction("A", "B", "C");
            r.AddTransaction("A", "B", "C");
            r.AddTransaction("A", "B", "C", "D");
            r.AddTransaction("A", "B", "C", "E");
            r.AddTransaction("A", "C", "E");
            r.AddTransaction("A", "C", "E");
            r.AddTransaction("A", "B", "D");
            r.AddTransaction("A", "B", "D");
            r.AddTransaction("A", "B", "D", "E");
            r.AddTransaction("B", "C");
            r.AddTransaction("B", "C");
            r.AddTransaction("B", "C", "D");
            r.AddTransaction("B", "C", "E");
            r.AddTransaction("B", "D", "E");

            string res = "";
            foreach (var t in r.GetAllItems())
                res += bulidTestResult(r,t) + "--------------- \r\n";

            res += "\r\n\r\n" + (r.Probability("START", "C"));
            return res;
        }

        static string bulidTestResult(MarkovChain<string> r, string v)
        {
            string s = v + "\r\n" + r.GetItem(v).Occurrence + "\r\n";
            foreach (var t in r.PredicteNext(v))
                s += "[" + t.State + "] - " + t.Value + " - " + t.Occurrence + " - " + r.GetItem(v).ProbabilityOf(t) + "\r\n";
            return s + "\r\n";
        }
    }
}
