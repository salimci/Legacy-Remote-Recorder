using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote
{
    public abstract class LinuxTerminalRecorderContext : TerminalRecorderContext
    {
        public LinuxTerminalRecorderContext()
            : this(null)
        {
        }

        public LinuxTerminalRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
            DirectorySeparatorChar = "/";
            InputRecord = new TextRecord();
        }

        public override string CommandReadRecords
        {
            get { return "if echo \"@NODE\"|grep -q \"\\.gz$\"; then gunzip -c \"@NODE\"|sed -n \"@FROM,@TOp;\"; else sed -n \"@FROM,@TOp;\" \"@NODE\";fi"; }
        }
        public override string CommandListFiles
        {
            get { return "find '@NODE' -maxdepth 1|while read -r a; do stat -c '/;%i;%W;%X;%Y;%Z;%F;%n' \"$a\"; done"; }
        }

        public override string CommandFileSystemInfo
        {
            get { return "msg=\"$(if [[ -e '@NODE' ]]; then stat -c '/;%i;%W;%X;%Y;%Z;%F;%n' '@NODE' 2>&1; fi)\";echo $?\";$msg\""; }
        }

        public override string CommandParentOf
        {
            get { return "msg=$(echo '@NODE'|sed 's/\\/\\/\\/*/\\//g; s/\\/$//g; s/\\(.*\\)\\/[^\\/]*$/\\1/g' 2>&1);echo \"/;$?\"\";$msg\""; }
        }
    }
}
