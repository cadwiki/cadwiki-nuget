#unignore AssemblyInfo changes in git

find ./ -name '*AssemblyInfo.*' -print0 | while read -d $'\0' f; do
	echo -e "$f";
    git update-index --no-skip-worktree "$f";
done
