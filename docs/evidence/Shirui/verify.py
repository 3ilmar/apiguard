"""Validate compact evidence only; never run archived app/test scripts.

Writes this collection's links.csv, tree.txt, validation.json and current manifest
metadata. Archived evidence and all original repository files are read-only.
"""
import collections, csv, datetime as dt, hashlib, io, json, os, pathlib, re, shutil, subprocess, tempfile, urllib.parse, zipfile
OUT=pathlib.Path(__file__).resolve().parent
ROOT=OUT.parent.parent.parent
BASE="bd9cc4340f67fb75d2db36a8294d66cfeda6b9a9"
MANIFEST=OUT/"evidence-manifest.csv"
FIELDS=["Evidence ID","Assignment responsibility","Test case or requirement","Description","Original path","New path",
        "Evidence type","Execution date, if recorded","Result or status","Source commit or PR, if known",
        "Original contributor or tester","SHA-256 hash","Bytes","Original last-write UTC","Record kind",
        "Storage","Archive member","Canonical evidence ID","Original link","Previous collection path"]
def sha(p):
    with p.open("rb") as f:return hashlib.file_digest(f,"sha256").hexdigest()
def csvwrite(p,rows,fields):
    with p.open("w",encoding="utf-8-sig",newline="") as f:
        w=csv.DictWriter(f,fieldnames=fields);w.writeheader();w.writerows(rows)
def git(*args):return subprocess.check_output(["git",*args],cwd=ROOT).decode("utf-8").strip()
def local_links(body,parent):
    for match in re.finditer(r"!?\[[^\]\n]*\]\((<[^>]+>|[^)\n]+)\)",body):
        link=match.group(1).strip().strip("<>")
        if not link.startswith(("https://","http://","mailto:","#","codex:")):
            yield link,(parent/urllib.parse.unquote(link.split("#")[0].split("?")[0])).resolve()
def main():
    assert OUT.resolve()==(ROOT/"docs/evidence/Shirui").resolve()
    assert git("branch","--show-current")=="codex/shirui-assignment2-evidence"
    with MANIFEST.open(encoding="utf-8-sig",newline="") as f:
        reader=csv.DictReader(f);assert reader.fieldnames==FIELDS
        rows=[r for r in reader if r["Record kind"]!="Current collection metadata"]
    errors=[];archives={};payload_cache={};direct_paths=set();members=collections.defaultdict(set)
    def payload(r):
        key=(r["New path"],r["Archive member"])
        if key not in payload_cache:
            p=ROOT/r["New path"]
            if r["Archive member"]:
                if p not in archives:archives[p]=zipfile.ZipFile(p)
                payload_cache[key]=archives[p].read(r["Archive member"])
                members[p].add(r["Archive member"])
            else:payload_cache[key]=p.read_bytes();direct_paths.add(p)
        return payload_cache[key]
    ids={r["Evidence ID"]:r for r in rows}
    assert len(ids)==len(rows)==1768
    for r in rows:
        try:
            b=payload(r)
            if hashlib.sha256(b).hexdigest()!=r["SHA-256 hash"] or len(b)!=int(r["Bytes"]):errors.append("Payload mismatch: "+r["Evidence ID"])
            c=ids[r["Canonical evidence ID"]]
            if c["SHA-256 hash"]!=r["SHA-256 hash"]:errors.append("Alias mismatch: "+r["Evidence ID"])
            if r["Record kind"]=="Byte-exact copy":
                original=ROOT/r["Original path"]
                if not original.is_file() or sha(original)!=r["SHA-256 hash"]:errors.append("Original mismatch: "+r["Original path"])
                if not (OUT/r["Original link"]).resolve()==original.resolve():errors.append("Original link mismatch: "+r["Evidence ID"])
                if r["Storage"]=="Direct copy" and original.stat().st_mtime_ns!=(ROOT/r["New path"]).stat().st_mtime_ns:errors.append("Direct timestamp mismatch: "+r["Evidence ID"])
        except Exception as e:errors.append(r["Evidence ID"]+": "+str(e))
    for p,z in archives.items():
        if z.testzip() is not None:errors.append("Archive CRC failure: "+str(p))
        names=z.namelist()
        if len(names)!=len(set(names)) or set(names)!=members[p]:errors.append("Archive inventory mismatch: "+str(p))
        if any("/" in n or "\\" in n or len(n)>32 for n in names):errors.append("Unsafe/deep archive member: "+str(p))
    baseline=json.loads((OUT/"08-git/originals.json").read_text(encoding="utf-8"))
    for r in baseline["baseline"]:
        p=ROOT/r["path"]
        if not p.is_file() or sha(p)!=r["sha256"] or p.stat().st_mtime_ns!=r["mtime_ns"]:errors.append("Pre-existing file changed: "+r["path"])
    by_source={r["Original path"]:r for r in rows if r["Record kind"]=="Byte-exact copy"}
    link_rows=[]
    for r in rows:
        if r["Record kind"]!="Byte-exact copy" or not r["Original path"].lower().endswith(".md"):continue
        for link,target in local_links(payload(r).decode("utf-8-sig"), (ROOT/r["Original path"]).parent):
            try:source=target.relative_to(ROOT).as_posix()
            except ValueError:source=str(target)
            found=by_source.get(source)
            link_rows.append({"Document evidence ID":r["Evidence ID"],"Original document":r["Original path"],"Original link":link,
                "Original target":source,"Original exists":target.exists(),"Collected target":found["New path"] if found else "",
                "Archive member":found["Archive member"] if found else "","Scope":"Historical original-relative link; use source or mapped target"})
    csvwrite(OUT/"links.csv",link_rows,["Document evidence ID","Original document","Original link","Original target","Original exists","Collected target","Archive member","Scope"])
    with (OUT/"07-metrics/ShiruiFinalTestCases.csv").open(encoding="utf-8-sig",newline="") as f:master=list(csv.DictReader(f))
    if dict(collections.Counter(r["Status"] for r in master))!={"PASS":16,"NOT RUN":6}:errors.append("Master statuses changed")
    for f,counts,overall in [("initial-browser.json",{"PASS":16},"FAIL"),("before-browser.json",{"PASS":14,"FAIL":2},"FAIL"),("after-browser.json",{"PASS":16},"PASS")]:
        d=json.loads((OUT/"06-accessibility"/f).read_text(encoding="utf-8-sig"))
        if dict(collections.Counter(r["status"] for r in d["results"]))!=counts or d["overallAutomatedStatus"]!=overall:errors.append("SQA-20 stage changed: "+f)
    http=json.loads((OUT/"06-accessibility/http-summary.json").read_text(encoding="utf-8-sig"))
    if (http["passed"],http["failed"],http["status"])!=(28,1,"FAIL"):errors.append("HTTP failure not retained")
    assessment=(OUT/"05-usability/observer-assessment-session-eilmar.md").read_text(encoding="utf-8-sig")
    if "observer approved" not in assessment or "5/5" not in assessment:errors.append("SQA-19 approval absent")
    if len(list((OUT/"04-performance/visual").glob("r*.png")))!=5 or len(list((OUT/"04-performance/visual").glob("r[1-5].json")))!=5:errors.append("Five independent visual contexts absent")
    # Check native staging with long paths explicitly disabled using temporary indexes.
    # --dry-run does not touch the user's index or add Git objects.
    stage_probes=[]
    git_exes=[shutil.which("git"),str(pathlib.Path.home()/".cache/codex-runtimes/codex-primary-runtime/dependencies/native/git/cmd/git.exe")]
    actual_index=pathlib.Path(git("rev-parse","--git-path","index"))
    if not actual_index.is_absolute():actual_index=ROOT/actual_index
    for exe in dict.fromkeys(git_exes):
        if not exe or not pathlib.Path(exe).exists():continue
        # The workspace is writable even when Windows sandbox TEMP is not.
        # This transient directory is explicitly ignored and removed on exit.
        with tempfile.TemporaryDirectory(prefix=".stage-",dir=OUT) as temp:
            index=pathlib.Path(temp)/"index";shutil.copy2(actual_index,index)
            env=os.environ.copy();env["GIT_INDEX_FILE"]=str(index)
            p=subprocess.run([exe,"-c","core.longpaths=false","add","--dry-run","--","docs/evidence/Shirui"],cwd=ROOT,env=env,capture_output=True)
            stage_probes.append({"git":exe,"version":subprocess.check_output([exe,"--version"]).decode().strip(),"core.longpaths":False,
                "exit_code":p.returncode,"stderr":p.stderr.decode("utf-8",errors="replace"),"dry_run_output_lines":len(p.stdout.splitlines())})
            if p.returncode or p.stderr:errors.append("Short-path Git dry run failed: "+exe)
    names={p.relative_to(OUT).as_posix() for p in OUT.rglob("*") if p.is_file()}
    names.update(["tree.txt","validation.json"])
    (OUT/"tree.txt").write_text("docs/evidence/Shirui/\n"+"\n".join("  "+p for p in sorted(names))+"\n",encoding="utf-8")
    generated=[];mapped={r["New path"] for r in rows}
    for rel in sorted(names):
        p=OUT/rel;repo=p.relative_to(ROOT).as_posix()
        if repo in mapped and p.suffix!=".zip":continue
        self_ref=rel in ["evidence-manifest.csv","validation.json"]
        generated.append({"Evidence ID":f"SH-C{len(generated)+1:03}","Assignment responsibility":"Evidence integrity and Git path repair",
            "Test case or requirement":"Collection validation only; no application tests rerun","Description":rel,
            "Original path":"Generated for compact evidence organisation","New path":repo,"Evidence type":"Archive container" if p.suffix==".zip" else "Current collection metadata",
            "Execution date, if recorded":"Not an application test run","Result or status":"Current collection record",
            "Source commit or PR, if known":BASE,"Original contributor or tester":"Codex-assisted reorganisation requested by Shirui",
            "SHA-256 hash":"" if self_ref else sha(p),"Bytes":"" if self_ref else p.stat().st_size,
            "Original last-write UTC":"Not applicable","Record kind":"Current collection metadata","Storage":"Generated file",
            "Archive member":"","Canonical evidence ID":"","Original link":"","Previous collection path":""})
    csvwrite(MANIFEST,rows+generated,FIELDS)
    new_links=list(local_links((OUT/"README.md").read_text(encoding="utf-8"),OUT))
    broken=[link for link,target in new_links if not target.exists() and target!=OUT/"validation.json"]
    if broken:errors.append("New README broken links: "+repr(broken))
    # Test complete current physical coverage; ZIP members are separate logical rows.
    actual_files={p.relative_to(ROOT).as_posix() for p in OUT.rglob("*") if p.is_file()}
    mapped_files={r["New path"] for r in rows+generated}
    if actual_files|{(OUT/"validation.json").relative_to(ROOT).as_posix()}!=mapped_files:errors.append("Unmapped physical files")
    outside_changes=[p for p in git("diff","--name-only",BASE).splitlines() if not p.startswith("docs/evidence/Shirui/")]
    if outside_changes:errors.append("Tracked changes outside collection: "+repr(outside_changes))
    if git("rev-parse","HEAD")!=BASE:errors.append("HEAD changed")
    files=[OUT/rel for rel in names]
    max_path=max(len(str(p.resolve())) for p in files)
    if max_path>=200:errors.append("Path exceeds compact layout limit")
    result={"validated_utc":dt.datetime.now(dt.timezone.utc).isoformat(),"client_timezone":"Pacific/Auckland","status":"PASS" if not errors else "FAIL",
        "scope":"Evidence integrity/layout/Git dry-run only; no application tests rerun","errors":errors,"historical_records_checked":len(rows),
        "original_evidence_records":len(by_source),"unique_original_hashes":len({r["SHA-256 hash"] for r in rows if r["Record kind"]=="Byte-exact copy"}),
        "preexisting_files_checked":len(baseline["baseline"]),"unique_stored_payloads":len(payload_cache),"archive_containers":len(archives),
        "archive_members":sum(len(z.namelist()) for z in archives.values()),"storage_counts":dict(collections.Counter(r["Storage"] for r in rows)),
        "original_reference_only":0,"new_readme_links_checked":len(new_links),"new_readme_broken_links":broken,
        "historical_md_links_mapped":len(link_rows),"historical_unavailable_original_links":sum(not r["Original exists"] for r in link_rows),
        "max_absolute_path_characters":max_path,"paths_at_least_260":0 if max_path<260 else None,"physical_file_count":len(names),
        "manifest_record_count":len(rows)+len(generated),"git_dry_run_probes":stage_probes,
        "application_or_other_original_changes":outside_changes,"note":"validation/manifest omit self hashes to avoid circular references; archive rows hash the archive container, evidence rows hash member contents"}
    (OUT/"validation.json").write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    for z in archives.values():z.close()
    result["directory_bytes"]=sum(p.stat().st_size for p in OUT.rglob("*") if p.is_file())
    print(json.dumps(result,ensure_ascii=False,indent=2))
    if errors:raise SystemExit(1)
if __name__=="__main__":main()
