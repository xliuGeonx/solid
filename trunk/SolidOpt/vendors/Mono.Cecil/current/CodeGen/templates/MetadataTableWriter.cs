//
// MetadataTableWriter.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// <%=Time.now%>
//
// (C) 2005 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.Cecil.Metadata {

	using System;
	using System.Collections;

	using Mono.Cecil.Binary;

	internal sealed class MetadataTableWriter : BaseMetadataTableVisitor {

		MetadataRoot m_root;
		TablesHeap m_heap;
		MetadataRowWriter m_mrrw;
		MemoryBinaryWriter m_binaryWriter;

		public MetadataTableWriter (MetadataWriter mrv, MemoryBinaryWriter writer)
		{
			m_root = mrv.GetMetadataRoot ();
			m_heap = m_root.Streams.TablesHeap;
			m_binaryWriter = writer;
			m_mrrw = new MetadataRowWriter (this);
		}

		public MetadataRoot GetMetadataRoot ()
		{
			return m_root;
		}

		public override IMetadataRowVisitor GetRowVisitor ()
		{
			return m_mrrw;
		}

		public MemoryBinaryWriter GetWriter ()
		{
			return m_binaryWriter;
		}

		void InitializeTable (IMetadataTable table)
		{
			table.Rows = new RowCollection ();
			m_heap.Valid |= 1L << table.Id;
			m_heap.Tables.Add (table);
		}

		void WriteCount (int rid)
		{
			if (m_heap.HasTable (rid))
				m_binaryWriter.Write (m_heap [rid].Rows.Count);
		}

<% $tables.each { |table| %>		public <%=table.table_name%> Get<%=table.table_name%> ()
		{
			<%=table.table_name%> table = m_heap [<%=table.table_name%>.RId] as <%=table.table_name%>;
			if (table != null)
				return table;

			table = new <%=table.table_name%> ();
			InitializeTable (table);
			return table;
		}

<% } %>		public override void VisitTableCollection (TableCollection coll)
		{
<% $stables.each { |table|  %>			WriteCount (<%=table.table_name%>.RId);
<% } %>		}
	}
}
