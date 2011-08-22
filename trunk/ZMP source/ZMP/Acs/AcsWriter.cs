﻿namespace ZMP.Acs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ZMP.Decorate;

    public class AcsWriter
    {        
        private TextWriter writer;

        public AcsWriter(TextWriter writer)
        {
            this.writer = writer;

            this.writer.WriteLine(Program.FileHeader);

            this.writer.WriteLine("#include \"zcommon.acs\"");
        }

        public void WriteCheckActorClass2(IEnumerable<Actor> actors)
        {
            var orderedActors =
                (from actor in actors
                orderby actor.ID
                select actor).ToList();

            int maxUsedId = orderedActors.Last().ID;

            var idToActorName = Enumerable.Range(0, maxUsedId + 1).ToDictionary(p => p, p => new { Name = "Unknown", Tag = "Unknown" });

            foreach (var actor in actors)
            {
                idToActorName[actor.ID] = new { Name = actor.Name, Tag = actor.Tag };
            }

            this.writer.WriteLine("str __actorClassTable[" + (maxUsedId + 2) + "] = {");

            foreach (var pair in from pair in idToActorName orderby pair.Key select pair)
            {
                this.writer.WriteLine("\"" + pair.Value.Name + "\",");
            }

            this.writer.WriteLine("\"Last\"};");

            this.writer.WriteLine("str __actorTagTable[" + (maxUsedId + 2) + "] = {");

            foreach (var pair in from pair in idToActorName orderby pair.Key select pair)
            {
                this.writer.WriteLine("\"" + pair.Value.Tag + "\",");
            }

            this.writer.WriteLine("\"Last\"};");

            this.writer.WriteLine(@"
                function int GetActorClassId(int tid){                    
                    if(tid == 0){
                        return CheckInventory(" + "\"__classId\"" + @");                    
                    } else {
                        return CheckActorInventory(tid, " + "\"__classId\"" + @");                    
                    }
                    return 0;
                }
                function bool CheckActorClass2(int tid, int class){                    
                    int classId = GetActorClassId(tid);
                    if(classId > " + maxUsedId + @" || classId < 0) return false;
                    return __actorClassTable[classId] == class;
                }
                function int GetActorClass(int tid){
                    int classId = GetActorClassId(tid);
                    if(classId > " + maxUsedId + @" || classId < 0) return " + "\"unknown\"" + @";
                    return __actorClassTable[classId];
                }
                function int GetActorTag(int tid){
                    int classId = GetActorClassId(tid);
                    if(classId > " + maxUsedId + @" || classId < 0) return " + "\"Unknown\"" + @";
                    return __actorTagTable[classId];
                }
            ");
        }

        public void WriteGetWeapon(IEnumerable<Actor> actors)
        {
            var weapons =
                from actor in actors
                where actor.Name == "weapon"
                from weapon in actor.Descendants
                select weapon;

            this.writer.WriteLine("function int GetWeapon(void){");

            foreach (Actor weapon in weapons)
            {
                this.writer.WriteLine("if(CheckWeapon(\"" + weapon.Name + "\") == 1) return \"" + weapon.Tag + "\";");
            }

            this.writer.WriteLine("return \"Unknown\";}");
        }

        public void WriteGetCustomProperty(IEnumerable<Actor> actors)
        {
            var actorsWithCP =
                from actor in actors
                where actor.GetActualCustomProperties().Count > 0
                select actor;

            this.writer.WriteLine("function int GetCustomProperty(int tid, int property){");

            foreach (Actor actor in actorsWithCP)
            {
                this.writer.WriteLine("if(CheckActorClass(tid, \"" + actor.Name + "\")){");

                foreach (var property in actor.GetActualCustomProperties())
                {
                    foreach (string value in property)
                    {
                        this.writer.WriteLine("if(property == \"" + property.Key + "\") return " + value + ";");
                    }
                }

                this.writer.WriteLine("return 0;}");
            }

            this.writer.WriteLine("return 0;}");
        }

        public void WriteGetWeaponCustomProperty(IEnumerable<Actor> actors)
        {
            var weaponsWithCP =
                from actor in actors
                where actor.Name == "weapon"
                from weapon in actor.Descendants
                where weapon.GetActualCustomProperties().Count > 0
                select weapon;

            this.writer.WriteLine("function int GetWeaponCustomProperty(int property){");

            foreach (Actor actor in weaponsWithCP)
            {
                this.writer.WriteLine("if(CheckWeapon(\"" + actor.Name + "\")){");

                foreach (var property in actor.GetActualCustomProperties())
                {
                    foreach (string value in property)
                    {
                        this.writer.WriteLine("if(property == \"" + property.Key + "\") return " + value + ";");
                    }
                }

                this.writer.WriteLine("return 0;}");
            }

            this.writer.WriteLine("return 0;}");
        }

        public void WriteOriginalContent(string content)
        {
            this.writer.WriteLine(content);
        }
    }
}